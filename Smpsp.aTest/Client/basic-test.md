# Manual test of the admin page 
Date of last test: 2025-12-13

Tested by: TW

Admin
---
1. Go to /admin
1. Sign in as admin (admin/admin)
1. Add 3 users
	- Name: AA - E-Mail: a@a.a - Veto level: 0
	- Name: BB - E-Mail: b@b.b - Veto level: 1
	- Name: CC - E-Mail: c@c.c - Veto level: 2
1. Add hashtags
	- #tag1
	- #tag2
	- #tag3
1. Restart the application
1. Check all added data

Sign in as a@a.a
---
1. Create 3 posts
	- Post 1 - any 2 image - Hashtags: #tag1
	- Post 2 - any video, image and text - Hashtags: #tag2
	- Post 3 - any text only - Hashtags: #tag3
1. Restart the application
1. Check all added data
1. Like post 1 and dislike post 2
1. Delete post 3
1. Sign out

Sign in as b@b.b
---
1. Are the votes correct?
	- Post 1: 1 like
	- Post 2: 1 dislike
1. Disklike post 1
1. Add veto to post 1
1. Delete post 2
1. Restart the application
1. Check all changed data
1. Sign out

Sign in as c@c.c
---
1. Are the votes correct?
	- Post 1: 1 like, 1 dislike, veto added
1. Delete veto from post 1
1. Restart the application
1. Check all changed data
1. Sign out