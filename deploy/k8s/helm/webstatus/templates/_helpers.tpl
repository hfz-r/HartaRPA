{{/*
Expand the name of the chart.
*/}}
{{- define "webstatus.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Create a default fully qualified app name.
We truncate at 63 chars because some Kubernetes name fields are limited to this (by the DNS naming spec).
If release name contains chart name it will be used as a full name.
*/}}
{{- define "webstatus.fullname" -}}
{{- if .Values.fullnameOverride }}
{{- .Values.fullnameOverride | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- $name := default .Chart.Name .Values.nameOverride }}
{{- if contains $name .Release.Name }}
{{- .Release.Name | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- printf "%s-%s" .Release.Name $name | trunc 63 | trimSuffix "-" }}
{{- end }}
{{- end }}
{{- end }}

{{/*
Create chart name and version as used by the chart label.
*/}}
{{- define "webstatus.chart" -}}
{{- printf "%s-%s" .Chart.Name .Chart.Version | replace "+" "_" | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Common labels
*/}}
{{- define "webstatus.labels" -}}
helm.sh/chart: {{ include "webstatus.chart" . }}
{{ include "webstatus.selectorLabels" . }}
{{- if .Chart.AppVersion }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
{{- end }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end }}

{{/*
Selector labels
*/}}
{{- define "webstatus.selectorLabels" -}}
app.kubernetes.io/name: {{ include "webstatus.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end }}

{{/*
Create the name of the service account to use
*/}}
{{- define "webstatus.serviceAccountName" -}}
{{- if .Values.serviceAccount.create }}
{{- default (include "webstatus.fullname" .) .Values.serviceAccount.name }}
{{- else }}
{{- default "default" .Values.serviceAccount.name }}
{{- end }}
{{- end }}

{{/*
Get the protocol
*/}}
{{- define "webstatus.protocol" -}}
{{- if .Values.common.infra.tls.enabled }}
{{- printf "%s" "https" }}
{{- else }}
{{- printf "%s" "http" }}
{{- end }}
{{- end }}

{{/*
Get internal host ip to access container resources
*/}}
{{- define "file-api.internalHost" -}}
{{- if .Values.hostAlias.hostname }}
{{- printf "%s" .Values.hostAlias.hostname }}
{{- else }}
{{- printf "%s" "localhost" }}
{{- end }}
{{- end }}