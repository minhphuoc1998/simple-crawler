## Simple crawler
Crawl articles from some famous sites

## Installation
### Requirement
* Dotnet 8.0^
```dotnet restore```
* Chromium
```pwsh playwright.ps1 install chromium```
### Build & Run
```dotnet build && dotnet run```

## Config & Setting
### Loader Task Setting
Load HTMl for a site
* Interval: interval to run these tasks (in `s`).
* BatchSize: number of tasks to run at once
### Crawler Setting
Initialize Crawling task for sites (vnexpress and tuoitre)
* Interval: interval to fetch all articles from a site (should be `60000`(s) or 1 day)
* Initialize: initialize fetching service
### Load Config:
* `LoadType`:
    * `BROWSER`: slow load with browser
    * `HTTP`: fast load with http request
* `Actions`: list of actions to perform when load with browser
    * `SCROLL`: not support scrolling in an element yet

Note: move to config service later
### Parse Config
* `ParseType`:
    * `LINK`: find the url from parent dom and try to navigate to another page (will support case wihout needs of redirect (e.g. get id of a category))
    * `PAGINATION`: find the pagination list from the parent url (not supported from dom yet)
        * `fixed-url-with-page`: add postfix to parent url (not supported for go-to-end type)


    * `GROUP`: get JSON from a page (current: `depth = 1`)
    * `CUSTOM`: custom with some additional config
* `Rules`: CSS selector and type for parse value
* `Children`: config for child parser
* `Multiple`: (not yet)
* `CallbackType`: (ingore this)

Note: move to config service later
