<br />
<div align="center">
    <img src="res/wall-clock.png" alt="Logo" width="150" height="150">
    <h3 align="center">IdleMon</h3>
    <p align="center">
      Monitor Windows user idle times
    </p>
</div>

## About

IdleMon writes a JSON file to the root of the current user's profile (`~/idletime.json`) with their current idle time and other relevant information.

Supports `/install` and `/uninstall` switches which install IdleMon as a `User-owned` Windows service. This means that a new user-scoped instance of IdleMon will be spawned in the background whenever a user logs on, and will be removed when a user logs off.

### JSON Format

```json
{
    "Username":"user",
    "Domain":"domain",
    "IdleTime":"00:00:00.2809998",
    "IsAdmin": false,
    "IsDomainAdmin": false,
    "IsEnterpriseAdmin": false
}
```
#### Username
The username of the audited user.

#### Domain
The domain (if any) of the audited user.

#### IdleTime
The `TimeSpan` representation of the amount of time that the audited user has been idle.

#### IsAdmin
`true` if the audited user is running IdleMon in an elevated session.

#### IsDomainAdmin
`true` if the audited user is a Domain Admin.

#### IsEnterpriseAdmin
`true` if the audited user is an Enterprise Admin.

## Installation

```shell
IdleMon.exe /Install
```

## Uninstallation

```shell
IdleMon.exe /Uninstall
```

## Attributions
<a href="https://www.flaticon.com/free-icons/wall-clock" title="wall-clock icons">Wall-clock icons created by Freepik - Flaticon</a>