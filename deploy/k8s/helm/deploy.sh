#!/usr/bin/env bash

# http://redsymbol.net/articles/unofficial-bash-strict-mode
set -euo pipefail

usage() {
  cat <<END
#################### Deploys the $app_name to a Kubernetes cluster with Helm charts. ####################
Parameters:
  -d | --dns <dns or ip address>
    Specifies the external DNS/IP address of the Kubernetes cluster (default: localhost).
  -h | --help
    Displays this help text and exits the script.
  -n | --namespace <namespace name>
    Specifies the namespace name to deploy the app (default: harta-rpa).
  --build-image <build images with docker-compose>
    Specifies options to build images (default: false).
  --clean <uninstall previous build Helm charts>
    Specifies options to start cleaning previous build (default: false).
  --debug <debug with --dry-run and --debug >
    Specifies options to debug Helm charts before initial deploy (default: false).
  --infrastructure <install infrastructure>
    Specifies options to deploy infrastructure resources (default: false).
#########################################################################################################
END
}

app_name='harta-rpa'
build_images=''
clean=''
debug=''
dns='localhost'
infrastructure=''
namespace='harta-rpa'

while [[ $# -gt 0 ]]; do
  case "$1" in
  -d | --dns)
    dns="$2"
    shift 2
    ;;
  -h | --help)
    usage
    exit 1
    ;;
  -n | --namespace)
    namespace="$2"
    shift 2
    ;;
  --build-image)
    build_images='yes'
    shift
    ;;
  --clean)
    clean='true'
    shift
    ;;
  --debug)
    debug='--debug --dry-run'
    shift
    ;;
  --infrastructure)
    infrastructure='true'
    shift
    ;;
  *)
    echo "Unknown option $1"
    usage
    exit 2
    ;;
  esac
done

if [[ $build_images ]]; then
  echo "#################### Building the $app_name Docker images ####################"
  docker-compose -p ../../.. -f ../../../src/docker-compose.yml build

  # Remove temporary images
  docker rmi --force "$(docker images -qf "dangling=true")"
fi

if [[ $clean ]]; then
  echo "Cleaning previous helm releases..."
  if [[ -z $(helm ls -q --namespace $namespace) ]]; then
    echo "No previous releases found"
  else
    helm uninstall $(helm ls -q --namespace $namespace) -n $namespace
    echo "Previous releases deleted"
    waitsecs=10
    while [ $waitsecs -gt 0 ]; do
      echo -ne "$waitsecs\033[0K\r"
      sleep 1
      : $((waitsecs--))
    done
  fi
fi

echo "#################### Begin $app_name installation using Helm ####################"
infras=(rabbitmq redis-data seq sql-data)
charts=(envoy-proxy file-api ordering-api webstatus)

if [[ $infrastructure ]]; then
  for infra in "${infras[@]}"; do
    echo "Installing infrastructure: $infra"
    helm install "$infra" --values common.yaml --namespace $namespace --create-namespace $infra $debug
  done
fi

for chart in "${charts[@]}"; do
  echo "Installing: $chart"
  helm install "$chart" --values common.yaml --namespace $namespace --create-namespace $chart $debug
done

echo "FINISHED: Helm charts installed."
