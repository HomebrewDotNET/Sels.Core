---
name: puppet-module-creation
description: "Create and modify custom Puppet modules/components/resources. USE WHEN: Creating new sels_* modules, implementing classes/definitions, adding resources, or extending existing custom modules. All modules are stored in N:\\production\\modules with sels_ prefix. Follows Puppet best practices and existing codebase patterns."
---

# Custom Puppet Module Creation

## Overview

This skill guides the creation and modification of custom Puppet modules/components in your infrastructure. All custom modules are prefixed with `sels_` and stored in `N:\production\modules\`.

## Module Structure

All sels_* modules follow standard Puppet layout conventions:

```
sels_modulename/
├── manifests/
│   ├── init.pp                    # Main class definition
│   └── subfeature/                # Optional: organized classes
│       ├── class.pp
│       └── nested.pp
├── files/                         # Static file resources
│   └── script.sh
├── templates/                     # ERB/EPP templates
│   └── config.erb
├── metadata.json                  # Module metadata
└── README.md                       # Module documentation
```

## Directory Preferences

When adding new functionality:
1. **Use existing directories first** — extend `manifests/`, `files/`, or `templates/` folders
2. **Create organized subdirectories only when needed** — e.g., `manifests/deployment/`, `manifests/disk/` for logical grouping
3. **Reuse class names** — avoid creating redundant class definitions

## Puppet Best Practices

### 1. Class Definitions

Every module must have a main class (`sels_modulename`):

```puppet
class sels_modulename (
  String $required_param,
  String $optional_param = 'default_value',
  Boolean $enable_feature = false,
) {
  # Implementation here
}
```

**Guidelines:**
- Use typed parameters (String, Boolean, Array, Hash, etc.)
- Document parameters with descriptions (add comments above class)
- Use descriptive names
- Provide sensible defaults

### 2. Documentation

Add documentation comments above class definitions:

```puppet
# Manages users and user credentials
# 
# @param node_name
#   The name of the node being configured
# @param additional_groups
#   Additional groups to create
#
class sels_user_creds (
  String $node_name,
  Array[String] $additional_groups = [],
) {
```

### 3. Resource Declarations

Follow these patterns for all resources:

```puppet
# File resources
file { '/path/to/file':
  ensure  => file,
  owner   => 'root',
  group   => 'root',
  mode    => '0755',
  content => 'file content',
}

# Exec resources
exec { 'meaningful-title':
  command     => '/usr/bin/some-command arg',
  path        => '/bin:/usr/bin',
  refreshonly => true,  # Only run on notify/subscribe
  unless      => 'test -f /some/condition',  # Prevent re-running
}

# Class inclusion
include sels_other::subclass
require sels_other::subclass  # Explicit ordering
```

**Guidelines:**
- Use meaningful resource titles for exec resources
- Always specify `path` for exec resources
- Use `refreshonly` for dependent resources
- Include `unless` or `onlyif` to prevent unnecessary runs
- Use `notify` and `subscribe` for ordering relationships
- Avoid hard-coded paths — use facts and parameters

### 4. OS and Platform Conditionals

Use `$facts['os']['family']` for platform detection:

```puppet
case $facts['os']['family'] {
  /^(Debian|Ubuntu)$/: {
    # Linux Debian-based implementation
  }
  'Windows': {
    # Windows implementation (uses PowerShell)
  }
  default: {
    fail("Unsupported OS family: ${facts['os']['family']}")
  }
}
```

### 5. Resource Relationships

Use relationships to manage ordering:

```puppet
# Notification (soft dependency)
exec { 'build':
  command => '/usr/bin/build',
  notify  => Service['app'],  # Service refreshes on change
}

# Requirement (hard dependency)
file { '/app/config':
  ensure  => file,
  require => Package['app'],  # Package must be installed first
}

# Arrow notation for longer chains
File['/config'] -> Service['myservice']
```

### 6. Variables and Interpolation

```puppet
# Simple variable assignment
$app_dir = '/opt/myapp'

# String interpolation
$message = "Application installed at ${app_dir}"

# Facts interpolation
$os_family = $facts['os']['family']
$hostname = $facts['hostname']
```

## Constraint Rules

### ✅ **Allowed:** Only `sels_*` module modifications

- Creating new `sels_*` modules
- Extending existing `sels_*` modules
- Adding classes, resources, files to `sels_*` modules
- Modifying metadata.json in `sels_*` modules

### ❌ **Prohibited:** No changes outside custom modules

- Do NOT modify non-sels_* modules (puppetlabs modules, third-party, etc.)
- Do NOT modify production manifest files in `manifests/` directory
- Do NOT change directory structure of standard modules

## Common Patterns in Existing Modules

### From sels_utils:
- Platform-specific exec commands with proper guards
- Reboot resource management (trigger, finish states)
- Systemd daemon reload pattern
- Managed file tracking via exec

### From sels_scripts:
- Simple module entry point (minimal init.pp)
- File distribution via `files/` directory
- Script execution and lifecycle management

### From sels_configs, sels_services:
- Configuration file management with templates
- Service lifecycle (start, enable, refresh)
- Configuration validation before restart

## File Organization Examples

**Simple module** (single class):
```
sels_simple/
├── manifests/init.pp
└── README.md
```

**Complex module** (organized by feature):
```
sels_complex/
├── manifests/
│   ├── init.pp
│   ├── deployment/
│   │   ├── linux.pp
│   │   └── windows.pp
│   ├── config/
│   │   └── setup.pp
│   └── management/
│       └── utils.pp
├── files/
│   └── scripts/
└── templates/
    └── configs/
```

## Implementation Checklist for New Modules

When creating a new `sels_*` module:

- [ ] Module name prefixed with `sels_`
- [ ] Created in `N:\production\modules\`
- [ ] Main class defined in `manifests/init.pp`
- [ ] Typed parameters with defaults documented
- [ ] Platform-specific logic uses `$facts['os']['family']`
- [ ] Exec resources have `path` and `unless`/`onlyif`
- [ ] Resource relationships explicitly defined with `->`, `notify`, `subscribe`
- [ ] No hard-coded paths (use facts/parameters)
- [ ] Comments document complex logic
- [ ] No external module dependencies unless necessary

## Related Modules

Existing `sels_*` modules in your infrastructure:
- `sels_certs` — Certificate management
- `sels_configs` — Configuration file deployment
- `sels_docker_files` — Docker file resources
- `sels_installers` — Installation and setup
- `sels_scripts` — Script distribution and execution
- `sels_services` — Service lifecycle management
- `sels_user_creds` — User and credential management
- `sels_utils` — System utilities and common commands

When extending or combining features, consider if functionality belongs in an existing module or requires a new one.

## Tips

1. **Test platform compatibility** — Always include both Linux (`Debian|Ubuntu`) and Windows (`Windows`) conditionals
2. **Use refresh guards** — Prevent resource churn with `refreshonly => true` and `unless` guards
3. **Explicit ordering** — Don't rely on implicit ordering; use `->`, `notify`, `require`
4. **Parameter validation** — Use typed parameters to catch errors early
5. **Reuse existing patterns** — Look at sels_utils for common patterns like reboot, exec, and file management
