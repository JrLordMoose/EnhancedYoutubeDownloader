# Installation Guide - Enhanced YouTube Downloader

## Table of Contents
1. [Windows SmartScreen Warning](#windows-smartscreen-warning)
2. [Installation Steps](#installation-steps)
3. [Uninstallation](#uninstallation)
4. [Troubleshooting](#troubleshooting)
5. [FAQ](#faq)

---

## Windows SmartScreen Warning

### Why You See This Warning

When you download and run the installer, Windows may show a **red warning screen** that says:

> **"Windows protected your PC"**
> Microsoft Defender SmartScreen prevented an unrecognized app from starting.

**This is normal and safe.** Here's why:

- ✅ The installer is **completely safe** and virus-free
- ✅ This warning appears because the installer is **not digitally signed** with a paid certificate ($200-500/year)
- ✅ Many free, open-source applications show this warning
- ✅ The source code is publicly available on GitHub for inspection
- ✅ Windows is just being cautious with unsigned executables

### How to Bypass the Warning (Safe)

Follow these simple steps:

#### Step 1: Download the Installer
Download `EnhancedYoutubeDownloader-Setup-v1.0.0.exe` from the [GitHub Releases page](https://github.com/JrLordMoose/EnhancedYoutubeDownloader/releases/latest).

#### Step 2: Run the Installer
Double-click the downloaded file. You'll see the blue warning screen:

```
Windows protected your PC

Microsoft Defender SmartScreen prevented an unrecognized app from starting.
Running this app might put your PC at risk.

[Don't run]
```

#### Step 3: Click "More info"
Click the small text that says **"More info"** on the warning screen.

#### Step 4: Click "Run anyway"
After clicking "More info", a new button appears at the bottom:

```
[Run anyway]
```

Click **"Run anyway"** to proceed with installation.

### Why This Is Safe

1. **Open Source**: All source code is available at https://github.com/JrLordMoose/EnhancedYoutubeDownloader
2. **No Malware**: You can scan the installer with any antivirus software
3. **Community Verified**: Public GitHub repository with transparent development history
4. **Standard Practice**: Most free, indie, and open-source Windows applications show this warning

### Alternative: Get a Signed Version

If your organization requires digitally signed executables, please contact the developer about purchasing a code signing certificate. This would eliminate the warning but comes at a significant annual cost.

---

## Installation Steps

### Standard Installation

Once you've bypassed the SmartScreen warning (if shown), follow these steps:

#### 1. Welcome Screen
The installer will open with the **Welcome** screen.
- Click **Next** to continue

#### 2. License Agreement
Read the license agreement (MIT License).
- Check **"I accept the agreement"**
- Click **Next**

#### 3. Select Destination Location
Choose where to install Enhanced YouTube Downloader.
- **Default:** `C:\Program Files\Enhanced YouTube Downloader\`
- Click **Next** (or **Browse** to change location)

#### 4. Select Start Menu Folder
Choose the Start Menu folder name.
- **Default:** `Enhanced YouTube Downloader`
- Click **Next**

#### 5. Select Additional Tasks
Choose your preferences:

✅ **Create a desktop icon** (Recommended - Default: Checked)
- Creates a shortcut on your desktop for quick access

⬜ **Create desktop uninstaller shortcut** (Optional - Default: Unchecked)
- Adds an uninstall shortcut to your desktop

✅ **Launch Enhanced YouTube Downloader after installation** (Recommended - Default: Checked)
- Automatically opens the app when installation completes

Click **Next** after making your selections.

#### 6. Ready to Install
Review your choices:
- Destination location
- Start Menu folder
- Selected tasks

Click **Install** to begin installation.

#### 7. Installation Progress
The installer will:
- Extract files (~39 MB with .NET 9.0 runtime)
- Create shortcuts
- Register with Windows Add/Remove Programs

This takes 10-30 seconds depending on your system.

#### 8. Completing Setup
Installation complete!

- If you checked "Launch after installation", the app will open automatically
- Click **Finish** to close the installer

---

## What Gets Installed

### Installation Directory
**Location:** `C:\Program Files\Enhanced YouTube Downloader\`

**Contents:**
- `EnhancedYoutubeDownloader.exe` - Main application
- `unins000.exe` - Uninstaller (now in root directory!)
- `ffmpeg.exe` - Media processing tool
- All .NET 9.0 runtime files and dependencies

### Shortcuts Created

#### Desktop (if selected)
- **Enhanced YouTube Downloader** - Launch application
- **Uninstall Enhanced YouTube Downloader** - Quick uninstall (if selected)

#### Start Menu
- **Enhanced YouTube Downloader** - Launch application
- **Uninstall Enhanced YouTube Downloader** - Uninstall application

#### Windows Integration
- **Add/Remove Programs** - Listed as "Enhanced YouTube Downloader v1.0.0"
- **Windows Settings → Apps** - Appears in installed apps list

---

## Uninstallation

You have **4 easy ways** to uninstall:

### Method 1: Desktop Shortcut (if created during install)
1. Double-click **"Uninstall Enhanced YouTube Downloader"** on your desktop
2. Confirm uninstallation
3. Done!

### Method 2: Start Menu
1. Open Start Menu
2. Find **Enhanced YouTube Downloader** folder
3. Click **"Uninstall Enhanced YouTube Downloader"**
4. Confirm uninstallation

### Method 3: Windows Settings
1. Open **Settings** (Windows + I)
2. Go to **Apps** → **Installed apps**
3. Find **"Enhanced YouTube Downloader"**
4. Click **three dots (...)** → **Uninstall**
5. Confirm uninstallation

### Method 4: Control Panel
1. Open **Control Panel**
2. Go to **Programs and Features**
3. Find **"Enhanced YouTube Downloader"**
4. Click **Uninstall**
5. Confirm uninstallation

### Method 5: Installation Directory
1. Open File Explorer
2. Navigate to `C:\Program Files\Enhanced YouTube Downloader\`
3. Double-click **`unins000.exe`**
4. Confirm uninstallation

### Data Cleanup Option

During uninstallation, you'll be asked:

> **Do you want to remove all application data including download history and settings?**
>
> Choose "Yes" to completely remove all data.
> Choose "No" to keep your settings and download history.

**What gets removed with "Yes":**
- Download history
- Saved settings
- Cached video metadata
- Authentication tokens
- All user data in `%LocalAppData%\Enhanced YouTube Downloader\`

**What stays if you choose "No":**
- All settings and data remain for future reinstalls

---

## Troubleshooting

### Issue: "Windows protected your PC" warning won't go away
**Solution:** Follow the [SmartScreen bypass steps](#how-to-bypass-the-warning-safe) above. Make sure you click "More info" first, then "Run anyway".

### Issue: Installer says "Access Denied"
**Solution:**
1. Right-click the installer
2. Select **"Run as administrator"**
3. Follow normal installation steps

### Issue: Can't find the uninstaller
**Solutions:**
- Check Desktop for uninstall shortcut (if you selected it during install)
- Check Start Menu under "Enhanced YouTube Downloader" folder
- Go to Windows Settings → Apps → Enhanced YouTube Downloader
- Navigate to installation folder: `C:\Program Files\Enhanced YouTube Downloader\unins000.exe`

### Issue: Application won't launch after installation
**Solution:**
1. Make sure installation completed successfully
2. Check Windows Event Viewer for error details
3. Try running as administrator: Right-click app → "Run as administrator"
4. Report issue at https://forms.gle/PiFJk212eFwrFB8Z6

### Issue: Antivirus blocks the installer
**Solution:**
Some overly cautious antivirus programs may block unsigned executables:
1. Temporarily disable your antivirus
2. Run the installer
3. Re-enable your antivirus
4. Add `C:\Program Files\Enhanced YouTube Downloader\` to antivirus exclusions

---

## FAQ

### Q: Is this installer safe?
**A:** Yes, completely safe. The SmartScreen warning is standard for unsigned open-source applications. You can verify the source code on GitHub or scan the installer with any antivirus tool.

### Q: Why isn't the installer signed?
**A:** Code signing certificates cost $200-500 per year. This is a free, open-source project, so we can't justify that expense. The application is just as safe without signing.

### Q: Do I need to install .NET separately?
**A:** No! The installer is **self-contained** and includes .NET 9.0 runtime. No additional downloads needed.

### Q: Can I install without administrator privileges?
**A:** Yes, the installer uses **"lowest" privileges** and installs to your user directory if needed. You don't need admin rights.

### Q: What if I want to move the installation later?
**A:**
1. Uninstall the current version
2. Run the installer again
3. Choose a new installation directory during setup

### Q: How do I update to a new version?
**A:**
1. Download the new installer from GitHub Releases
2. Run it - it will automatically upgrade the existing installation
3. Your settings and data are preserved

### Q: Where is my download history stored?
**A:** `%LocalAppData%\Enhanced YouTube Downloader\` (typically `C:\Users\YourName\AppData\Local\Enhanced YouTube Downloader\`)

### Q: Can I use this installer on multiple computers?
**A:** Yes, the installer can be used on as many Windows computers as you want. It's free and open-source.

### Q: What Windows versions are supported?
**A:** Windows 10 (version 1809 or later) and Windows 11. The .NET 9.0 runtime requires these versions.

---

## Support & Resources

### Get Help
- **Bug Reports:** https://forms.gle/PiFJk212eFwrFB8Z6
- **GitHub Issues:** https://github.com/JrLordMoose/EnhancedYoutubeDownloader/issues
- **In-App Tutorial:** Click the "Tutorial" button in the app header

### Documentation
- **README:** https://github.com/JrLordMoose/EnhancedYoutubeDownloader#readme
- **Developer Docs:** https://github.com/JrLordMoose/EnhancedYoutubeDownloader/blob/main/CLAUDE.md
- **Release Notes:** https://github.com/JrLordMoose/EnhancedYoutubeDownloader/releases

### Download
- **Latest Release:** https://github.com/JrLordMoose/EnhancedYoutubeDownloader/releases/latest
- **All Releases:** https://github.com/JrLordMoose/EnhancedYoutubeDownloader/releases

---

## Security Notice

This application:
- ✅ Is **open-source** with publicly available code
- ✅ Does **NOT** collect any personal information
- ✅ Does **NOT** phone home or send telemetry
- ✅ Does **NOT** contain any malware or spyware
- ✅ Is **completely free** with no ads or tracking
- ✅ Runs **entirely on your local machine**
- ✅ Only communicates with YouTube to download videos

Your privacy and security are paramount.

---

**Enhanced YouTube Downloader - Professional YouTube video downloader with modern UI/UX**

*Version 1.0.0 | MIT License | Free & Open Source*
