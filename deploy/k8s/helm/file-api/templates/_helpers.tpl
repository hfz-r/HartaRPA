{{/*
Expand the name of the chart.
*/}}
{{- define "file-api.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Create a default fully qualified app name.
We truncate at 63 chars because some Kubernetes name fields are limited to this (by the DNS naming spec).
If release name contains chart name it will be used as a full name.
*/}}
{{- define "file-api.fullname" -}}
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
{{- define "file-api.chart" -}}
{{- printf "%s-%s" .Chart.Name .Chart.Version | replace "+" "_" | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Common labels
*/}}
{{- define "file-api.labels" -}}
helm.sh/chart: {{ include "file-api.chart" . }}
{{ include "file-api.selectorLabels" . }}
{{- if .Chart.AppVersion }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
{{- end }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end }}

{{/*
Selector labels
*/}}
{{- define "file-api.selectorLabels" -}}
app.kubernetes.io/name: {{ include "file-api.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end }}

{{/*
Create the name of the service account to use
*/}}
{{- define "file-api.serviceAccountName" -}}
{{- if .Values.serviceAccount.create }}
{{- default (include "file-api.fullname" .) .Values.serviceAccount.name }}
{{- else }}
{{- default "default" .Values.serviceAccount.name }}
{{- end }}
{{- end }}

{{/*
Get the protocol
*/}}
{{- define "file-api.protocol" -}}
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

{{/*
Get external (local) MS-SQL address
*/}}
{{- define "file-api.sqlExt" -}}
{{- if .Values.localDb }}
    {{- with .Values.localDb }}
        {{- printf "%s.%s.%s" .name $.Release.Namespace "svc" }}
    {{- end }}
{{- else }}
{{- printf "%s" "localhost" }}
{{- end }}
{{- end }}