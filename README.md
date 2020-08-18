# KeePassOTP
[![Version](https://img.shields.io/github/release/rookiestyle/keepassotp)](https://github.com/rookiestyle/keepassotp/releases/latest)
[![Releasedate](https://img.shields.io/github/release-date/rookiestyle/keepassotp)](https://github.com/rookiestyle/keepassotp/releases/latest)
[![Downloads](https://img.shields.io/github/downloads/rookiestyle/keepassotp/total?color=%2300cc00)](https://github.com/rookiestyle/keepassotp/releases/latest/download/keepassotp.plgx)\
[![License: GPL v3](https://img.shields.io/github/license/rookiestyle/keepassotp)](https://www.gnu.org/licenses/gpl-3.0)

KeePassOTP allows you to generate one time passwords (OTP). Both TOTP and HOTP are supported.

Once maintained, you can copy or auto-type the OTP.  
You may also create a QR code and scan it with an OTP app of your liking, e.g. andOTP, Google Authenticator, ...  

Functionality provided:
- Drag&Drop QR codes to add OTP
- Auto-Type placeholder
- Configurable hotkey to invoke Auto-Type OTP
- Column to display OTP or to indicate possible 2FA usage
- Compatibility with Google Authenticator, andOTP, ...
- Support TOTP and HOTP
- Secure storage of OTP secrets
- Auto-Type / Copy OTP using the KeePass tray icon

# Table of Contents
- [Configuration](#configuration)
- [Usage](#usage)
- [Translations](#translations)
- [Download and Requirements](#download-and-requirements)

# Configuration
KeePassOTP integrates into KeePass' options form.\
<img src="images/KeePassOTP%20-%20options.png" alt="Options" height="50%" width="50%"/>  
In the database-specific area you can choose the working mode of KeePassOTP.  

<img src="images/KeePassOTP%20-%20options%202.png" alt="Options 2" height="50%" width="50%"/>
The general area lets you define settings that are valid for all databases.

# Usage
OTP settings are entry-specific.  
To setup, change, copy or auto-type the OTP, you may use the context menu of the respective entry.

If you're interested in migration instructions to move from (or to) other OTP plugins or if you want to know more about some of the technical details, please have a look at the [Wiki](https://github.com/rookiestyle/keepassotp/wiki).

## Setup OTP data / Change OTP data  
Use the entry's context menu to setup/change OTP data.  

In the setup form, you can drag&drop the QR code image and have KeePassOTP parse it or you can enter the OTP secret manually (sometimes referred to as a seed) and you're ready to go.  

Got a string like this one?  
`otpauth://totp/Example:alice@google.com?secret=JBSWY3DPEHPK3PXP&issuer=Example`  
Paste the entire string, KeePassOTP can process this [otpauth-format](https://github.com/google/google-authenticator/wiki/Key-Uri-Format) as well.

As almost all sites require you to enter an OTP as part of the activation process, both current and next OTP are displayed here as well.  
<img src="images/KeePassOTP%20-%20setup%20simple.png" alt="Setup" />

## Use OTP data  
There are multiple convenient ways to use the one time passwords.

- Copy OTP 
  - using the context menu of the entry
  - doubleclicking the OTP displayed in the optional KPOTP column
  - using KeePass' tray icon
- Auto-Type
  - using the context menu of the entry - *Show additional auto-type menu commands* needs to be active
  - using a configurable placeholder - default placeholder: `{KPOTP}`
  - using a configurable hotkey working inside and outside KeePass
  - using KeePass' tray icon
 
# Translations
KeePassOTP is provided with English language built-in and allows usage of translation files.
These translation files need to be placed in a folder called *Translations* inside your plugin folder.
If a text is missing in the translation file, it is backfilled with English text.
You're welcome to add additional translation files by creating a pull request as described in the [wiki](https://github.com/Rookiestyle/KeePassOTP/wiki/Create-or-update-translations).

Naming convention for translation files: `KeePassOTP.<language identifier>.language.xml`\
Example: `KeePassOTP.de.language.xml`
  
The language identifier in the filename must match the language identifier inside the KeePass language that you can select using *View -> Change language...*\
This identifier is shown there as well, if you have [EarlyUpdateCheck](https://github.com/rookiestyle/earlyupdatecheck) installed

# Download and Requirements
## Download
Please follow these links to download the plugin file itself.
- [Download newest release](https://github.com/rookiestyle/keepassotp/releases/latest/download/KeePassOTP.plgx)
- [Download history](https://github.com/rookiestyle/keepassotp/releases)

If you're interested in any of the available translations in addition, please download them from the [Translations](Translations) folder.
## Requirements
* KeePass: 2.42

