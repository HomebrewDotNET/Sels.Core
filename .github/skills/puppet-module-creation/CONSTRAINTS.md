# Puppet Module Creation - Constraints & Rules

## File Access Rules

This document defines which files and modules the agent may modify when working with Puppet infrastructure.

### ✅ ALLOWED Operations

The agent MAY perform these operations:

1. **Create new sels_* modules**
   - Directory: `N:\production\modules\sels_*`
   - Files: `manifests/init.pp`, `manifests/**/*.pp`, `files/**`, `templates/**`, `metadata.json`, `README.md`

2. **Modify existing sels_* modules**
   - All subdirectories and files within `N:\production\modules\sels_*`
   - Adding new classes, resources, files
   - Updating metadata.json for custom modules
   - Extending existing functionality

3. **Read operations on any module**
   - Review other Puppet modules for patterns and best practices
   - Reference standard modules as examples
   - Review infrastructure code for context

### ❌ BLOCKED Operations

The agent MUST NOT perform these operations:

1. **Modify standard/third-party Puppet modules**
   - Prohibited directories: `N:\production\modules/*` (except sels_*)
   - Examples: puppetlabs modules, community modules, stdlib
   - Reason: These are managed dependencies; local patches create maintenance debt

2. **Modify production manifests**
   - Prohibited: `N:\production\manifests/*.pp`
   - Reason: Node definitions and Puppetfile assignments are infrastructure declarations
   - Changes require change control and need human review

3. **Create modules without sels_ prefix**
   - Any new module MUST be named `sels_*`
   - Reason: Distinguishes custom infrastructure code from managed dependencies

4. **Remove or rename sels_* modules**
   - Prohibited: Deletion of module directories
   - Reason: Requires dependency and impact analysis

### Validation Rules

Before suggesting or implementing changes, verify:

- [ ] Target module is named `sels_*` (for modifications/creation)
- [ ] Changes are within `N:\production\modules/sels_*/`
- [ ] No modifications to standard module directories
- [ ] No changes to `N:\production\manifests/`
- [ ] Resource declarations follow Puppet best practices
- [ ] Platform conditionals included for multi-OS support
- [ ] Parameters are typed and documented
- [ ] No hard-coded paths or assumptions

## Enforcement

If a user requests a change that violates these rules:

1. **Politely decline** the operation
2. **Explain the restriction** and why it exists
3. **Suggest an alternative approach** within allowed operations
4. **Example**: "I can't modify stdlib, but I can create a sels_wrapper module that augments its functionality"

## Examples

### ✅ Valid Request
"Create a new sels_ldap_auth module to manage LDAP authentication across nodes"
→ Create new module in `N:\production\modules\sels_ldap_auth/`

### ✅ Valid Request  
"Add Docker daemon configuration to sels_docker_files"
→ Modify/extend `N:\production\modules\sels_docker_files/`

### ❌ Invalid Request
"Fix a bug in the puppetlabs/stdlib module"
→ Must be fixed upstream or wrapped in a custom sels_* module

### ❌ Invalid Request
"Update the hive.pp node definition"
→ Changes to manifests require human review and change control
