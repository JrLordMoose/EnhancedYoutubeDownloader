# 🍪 Sign-in with a cookies.txt file

**Most people never need this.** Enhanced YouTube Downloader works without signing in for YouTube, TikTok, X/Twitter, Facebook, Dailymotion, Twitch, SoundCloud and direct video links. Set up a cookies file only if the app tells you a site needs you to sign in.

---

## When do I need it?

You need it when a download fails with **"Sign-in required"** (the app also asks you automatically), for example:

| Situation | Why |
|---|---|
| YouTube says **"Sign in to confirm you're not a bot"** | YouTube is suspicious of your network (VPN, shared/office Wi-Fi, lots of downloads). This often comes and goes. |
| **Private**, members-only or **age-restricted** videos | Only a signed-in account can watch them. |
| **Instagram**, **Reddit**, **Vimeo** | These sites now require an account for most videos. |

## What is a cookies.txt file?

When you sign in to a website, your browser stores small "cookies" that prove you're signed in. A **cookies.txt** file is a copy of those cookies for one website. The app hands it to the downloader so the download looks like it comes from you, signed in.

- ✅ It stays **on your computer**. The app never uploads it, copies it or reads it beyond passing it to the downloader.
- ⚠️ **Treat it like a password.** Anyone with the file can use that session of your account. Don't share it, email it or put it in a cloud folder others can see. A **secondary account** is a good idea.

## Why not just "Use Browser Cookies"?

On Windows, **Chrome, Edge, Brave, Opera and other Chromium browsers encrypt their cookies** so other apps can't read them. Selecting them fails with *"Couldn't Read Browser Cookies"*. **Firefox** is the only browser whose cookies the app can read directly. A cookies.txt file works with **any** browser, so we recommend it.

## Set it up (about 2 minutes)

1. **Install the extension** [Get cookies.txt LOCALLY](https://chromewebstore.google.com/search/Get%20cookies.txt%20LOCALLY) in Chrome, Brave or Edge. Use that exact name: it saves the file on your computer and doesn't upload it. Firefox users: "cookies.txt" by Lennon Hill.
2. **Allow it in private windows** (recommended, so the file lasts longer):
   - Brave/Chrome: open `brave://extensions` or `chrome://extensions`, then **Get cookies.txt LOCALLY → Details → Allow in Private / Allow in Incognito**.
3. **Open a private window** (Ctrl+Shift+N), go to the site (e.g. youtube.com) and **sign in**.
4. Click the extension icon (the puzzle piece) → **Export**. Save it somewhere private, e.g. `Documents\yt-cookies.txt`.
5. **Close the private window** without signing out.
6. In the app: **Settings → Advanced → Authentication → Cookies File → Browse** and pick the file → **Save**. When a download fails with "Sign-in required", the prompt's **Choose cookies.txt** button does the same thing.
7. Click **Restart** on the failed download.

> One file covers **one website**. For Instagram or Reddit, export a cookies.txt while on that site.

## It stopped working

| Symptom | Fix |
|---|---|
| "Sign in to confirm you're not a bot" again | The cookies **went stale**: YouTube rotated your session. Export a fresh file (steps 3–6). Exporting from a **private window** makes the file last much longer, because your normal browsing no longer rotates it. |
| "Couldn't Read Browser Cookies" | You turned on **Use Browser Cookies** with Chrome/Edge/Brave. Turn it off and use a cookies file (or Firefox). |
| Still failing with a fresh file | Make sure you were signed in when you exported, and that the file is for the right site. |

## Remove it

**Settings → Advanced → Authentication → Clear**, then delete the cookies.txt file. Signing out of that session in your browser also invalidates the file.

---

*Questions or problems? [Report an issue](https://github.com/JrLordMoose/EnhancedYoutubeDownloader/issues).*
