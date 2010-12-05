# TODO make this a simple ASP.NET app that we'll run with XSP or Cassini
require 'rubygems'
require 'sinatra'

get '/' do
  "You did: GET /"
end

post '/foo' do
  "You did: POST /foo #{ params.inspect }"
end
