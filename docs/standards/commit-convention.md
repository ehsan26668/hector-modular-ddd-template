# Commit Convention

## Format

`type(scope): summary`

## Rules

- lowercase type
- lowercase scope
- imperative summary (e.g., "add", not "added")
- no trailing period
- keep commits focused on a single logical change

## Allowed Types

- `docs`: Documentation only changes
- `feat`: A new feature
- `fix`: A bug fix
- `refactor`: A code change that neither fixes a bug nor adds a feature
- `test`: Adding missing tests or correcting existing tests
- `chore`: Changes to the build process or auxiliary tools/libraries
- `build`: Changes that affect the build system or external dependencies
- `ci`: Changes to CI configuration files and scripts

## Common Scopes

- `governance`, `adr`, `issues`
- `domain`, `application`, `infra`, `web`
- `architecture`, `testing`
- `ci`, `cd`, `deps`
