# K8Workeriino
![alt text](https://github.com/kitsun8/K8Workeriino/blob/master/screenshots/workeriino.PNG)

.NET Core application that fetches Overwatch competitive statistics from desired OWApi instance for each user in K8Directoriino database. Meant to be run once a day.

# What it does

Fetches and stores competitive stats for players in K8Directoriino database.
Meant to be used as a nightly single-run (updating leaderboards).


# Details
Uses Given OWApi (https://github.com/SunDwarf/OWAPI) to fetch Overwatch competitive stats for users.
Stores found data in K8Directoriino database.
Project runs with .NET Core
