DevDefined.Bitbucket.MMBot
==========================

A Simple MMBot script which will scan bitbucket and return details of any new comments or updated comments since the last scan.

Currently out of the box it will look back 120 commits only.  In the future I might make this configurable.

Please see the [MMBot website](https://github.com/mmbot/mmbot) for more info on what MMBot is (if you don't know already), and a big thanks to [PeteGoo](https://twitter.com/petegoo) for building MMBot :)

Installation
------------

In your mmbot installation folder (the folder above /packages and /scripts) run the following command:

    nuget install DevDefined.Bitbucket.MMBot -o packages

You should then see this:

	Attempting to resolve dependency 'Microsoft.AspNet.WebApi.Client (= 5.1.2)'.
	Attempting to resolve dependency 'Newtonsoft.Json (= 4.5.11)'.
	Attempting to resolve dependency 'Microsoft.Net.Http (= 2.2.13)'.
	Attempting to resolve dependency 'Microsoft.Bcl (= 1.1.3)'.
	Attempting to resolve dependency 'Microsoft.Bcl.Build (= 1.0.10)'.
	Installing 'Newtonsoft.Json 4.5.11'.
	Successfully installed 'Newtonsoft.Json 4.5.11'.
	Installing 'Microsoft.Bcl.Build 1.0.14'.
	Successfully installed 'Microsoft.Bcl.Build 1.0.14'.
	Installing 'Microsoft.Bcl 1.1.7'.
	Successfully installed 'Microsoft.Bcl 1.1.7'.
	Installing 'Microsoft.Net.Http 2.2.13'.
	Successfully installed 'Microsoft.Net.Http 2.2.13'.
	Installing 'Microsoft.AspNet.WebApi.Client 5.1.2'.
	Successfully installed 'Microsoft.AspNet.WebApi.Client 5.1.2'.
	Installing 'DevDefined.Bitbucket.MMBot 0.9.0.0'.
	Successfully installed 'DevDefined.Bitbucket.MMBot 0.9.0.0'.

Then open your mmbot.ini file and add a section like this:

	[BITBUCKET]
	USERNAME = bitbucket_login
	PASSWORD = bitbucket_password

If you now restart MMBot, on startup you will see a message that reads:

    INFO : Loading script BitbucketComments

Configuration
-------------

Once installed, you can now ask mmbot to show all the new commits / updated commits by running this command:

	mmbot bitbucket comments check <owner> <repoSlug>

Replacing owner and reposlug with the values for your bitbucket repository.

Last of all, you probably don't want to have to keep invoking this manually - so get mmbot to run it regularly, by issuing this command:

    mmbot repeat every 5m bitbucket comments check <owner> <repoSlug>

And you now have a handy stream of comment creates/updates for commits of which you are not the author.
