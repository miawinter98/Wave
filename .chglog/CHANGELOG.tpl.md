{{ range .Versions }}
## {{ .Tag.Name }} ({{ datetime "2006-01-02" .Tag.Date }})

{{ range .CommitGroups -}}
### {{ .Title }}
{{- $title := .Title }}

{{ range .Commits -}}
	* {{ (replace (upperFirst .Subject) $title "" 1) | trim | upperFirst }}
{{ end }}
{{ end -}}

{{- if .NoteGroups -}}
{{ range .NoteGroups -}}
### {{ .Title }}

{{ range .Notes }}
	{{ .Body }}
{{ end }}
{{ end -}}
{{ end -}}
{{ end -}}