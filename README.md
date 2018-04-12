![Travis-CI Build Status](https://travis-ci.org/robertruetz/MamaBird.svg?branch=master)
# MamaBird
Fake HTTP Server for use in testing where there is a need for parroted http responses

A Different Take
----------------
FakeHttpServer class is based on the HttpServer class by [@benyblack](https://github.com/benyblack) in the [rest-mock-core](https://github.com/benyblack/rest-mock-core) repository. The main difference is that MamaBird implements responses as a FIFO Queue for each route provided by the user. This allows multiple responses to be sent for each route in the order they are added. This is good for testing code that makes more than one request. 