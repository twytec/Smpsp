# Social media - Post selection process

The project is currently in the beta phase.

Many organisations wish to showcase their activities on social media platforms. 
However, particularly in larger companies, it is often not feasible to allow all 
employees to publish their own posts on the official channels.

A transparent and democratic selection process, in which all members of an organisation 
first submit their content to the community for approval before it is published, offers 
numerous advantages. It encourages participation and enables employees to gain insights 
into various perspectives within the organisation. At the same time, potentially 
problematic posts can be identified and prevented at an early stage.

SMPSP specifically supports this approach.

![app recordning](./assets/smpsp.gif)

## Voting process
```mermaid
flowchart TD
	A[Create post] --> B[Community voting]
	B --> C{Likes > dislikes}
	C -->|Yes| D[Post selected]
	C -->|No| E[Post not selected]
```

## Veto process
Only users with a veto level above 0 are permitted to issue a veto. 
A veto can only be overturned by someone whose veto level is higher than that of the 
person who placed the veto.

Veto level ranges from 0 to 2,147,483,647 :)

## Features
- [x] Simple user management
- [x] Passwordless authentication 
- [x] Create posts with text, images, videos and hashtags
- [x] Automatic conversion of images and videos that do not comply with web standards
- [x] Community voting on posts
- [x] Like and comment on posts
- [x] Veto and comment on posts
- [x] Download posts as a ZIP file
- [ ] Automatic posting on Instagram
- [ ] Automatic posting on Facebook
- [ ] Automatic posting on LinkedIn
- [ ] Automatic posting on X (formerly Twitter)
- [ ] Automatic posting on BlueSky
- [ ] Automatic posting on TikTok
- [ ] Automatic posting on YouTube
- [ ] Automatic posting on Mastodon

## Installation
SMPSP is a web-based application that runs on a server.

SMPSP should run continuously in the background so that background processes can be processed.

For detailed installation instructions, please refer to the official documentation: [Host and deploy ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy)

The simplest option is to find an ASP.NET Core hosting provider or Docker.

To process videos, the application requires FFmpeg and FFprobe for the respective operating system. Both files must be saved in the appdatas/ffmpeg folder. [FFmpeg](https://www.ffmpeg.org/)

## Getting started
After installation, open the admin page in a browser. https://your-domain/admin

Sign in with the default admin user:
- Username: admin
- Password: admin

Change the username and password of the default admin user. The username, password and all settings are stored in appdatas/mysettings.json.

Set the email server settings.

Create users.

Let's go