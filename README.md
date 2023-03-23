# EchoREST
A simple echo REST api, written in .NET, returning info about the incoming HTTP request

## How to use it

 1. Download the code & launch it __OR__ simply run the docker image `docker run -p 3000:80 bordinmartino/echorest`__OR__ simply access it using https://echorestbot.azurewebsites.net/
 2. Submit an HTTP request to the endopoint
 3. Check the history (last 10 requests) accessing __/history__ endpoint
 4. Check the history details accessing __/history/{id}__ endpoint
 
![enter image description here](https://raw.githubusercontent.com/martinobordin/EchoREST/master/Screenshot.png)
