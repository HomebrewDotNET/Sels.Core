# Puppet Module Creation - Quick Reference

## Create a New Module

```powershell
# Create directory structure
mkdir N:\production\modules\sels_mymodule\manifests
mkdir N:\production\modules\sels_mymodule\files
mkdir N:\production\modules\sels_mymodule\templates
```

Then create:
- `manifests/init.pp` — Main class
- `metadata.json` — Module metadata
- `README.md` — Documentation

## Basic Module Template

```puppet
class sels_mymodule (
  String $param1,
  String $param2 = 'default',
) {
  case $facts['os']['family'] {
    /^(Debian|Ubuntu)$/: {
      # Linux implementation
    }
    'Windows': {
      # Windows implementation
    }
    default: {
      fail("Unsupported OS: ${facts['os']['family']}")
    }
  }
}
```

## Common Patterns

### Exec Resource
```puppet
exec { 'meaningful-title':
  command     => '/path/to/command arg',
  path        => '/bin:/usr/bin:/usr/sbin',
  unless      => 'test -f /condition',
  refreshonly => true,
}
```

### File Resource
```puppet
file { '/path/to/file':
  ensure  => file,
  owner   => 'root',
  group   => 'root',
  mode    => '0755',
  content => file('sels_mymodule/filename'),
}
```

### Include Dependency
```puppet
require sels_other::class
include sels_another::class
```

### Ordering
```puppet
File['/app/config'] -> Service['app']  # Hard dependency
Service['app'] ~> Class ['app::restart']  # Notify dependency
```

## File Organization

**Simple Module**
```
sels_mymodule/
├── manifests/init.pp
└── README.md
```

**Complex Module**
```
sels_mymodule/
├── manifests/
│   ├── init.pp
│   ├── linux.pp
│   └── windows.pp
├── files/
│   └── script.sh
├── templates/
│   └── config.erb
├── metadata.json
└── README.md
```

## Platform Detection

```puppet
case $facts['os']['family'] {
  /^(Debian|Ubuntu)$/: { }  # Linux
  'Windows': { }             # Windows
  default: { fail(...) }
}
```

## Resource Relationships

| Operator | Meaning | Usage |
|----------|---------|-------|
| `->` | Hard dependency (before) | `Package['app'] -> Service['app']` |
| `~>` | Soft dependency (notify) | `File['/etc/app'] ~> Service['app']` |
| `require` | Hard dependency (declare) | `require Class['dependency']` |
| `subscribe` | Subscribe to notification | `subscribe => File['/config']` |

## Parameter Types

Common types:
- `String` — Text value
- `Integer` — Whole number
- `Boolean` — true/false
- `Array[String]` — List of strings
- `Hash[String, String]` — Key-value pairs
- `Optional[String]` — May be undef
- `Enum['opt1', 'opt2']` — One of options

## Validation Checklist

- [ ] Module name starts with `sels_`
- [ ] Located in `N:\production\modules\`
- [ ] Parameters are typed
- [ ] Includes Linux AND Windows support
- [ ] Exec resources have `path` and guard
- [ ] No hard-coded paths (use facts)
- [ ] Resources have explicit ordering
- [ ] Comments explain complex logic
- [ ] README documents parameters
- [ ] metadata.json includes dependencies

## Get Help

```
/help puppet-module-creation  # Show this skill
/puppet-module-creation      # Invoke the skill
```

## Key Facts

```puppet
$hostname = $facts['hostname']
$os_family = $facts['os']['family']
$os_name = $facts['os']['name']
$os_release = $facts['os']['release']['major']
```

## Common Mistakes

❌ Hardcoded paths → ✅ Use variables/facts
❌ Missing `path` in exec → ✅ Always specify `path` parameter
❌ No platform conditionals → ✅ Support Linux AND Windows
❌ Missing guards → ✅ Use `unless`/`onlyif` for idempotence
❌ Implicit ordering → ✅ Use `->` or `notify` explicitly
