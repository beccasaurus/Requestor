use Rack::Session::Cookie

run lambda { |env|
  request  = Rack::Request.new(env)
  response = Rack::Response.new

  path = request.path_info.empty? ? '/' : request.path_info
  verb = request.request_method

  env['rack.session']['timesRequested'] ||= 0
  env['rack.session']['timesRequested'] +=  1

  case path
  when '', '/'
    response.write "Hello World"
  when '/info'
    response.headers['Content-Type'] = 'text/plain'
    response.write "You did: #{verb} #{path}"
    response.write "\n\n"
    response.write "Times requested: #{env['rack.session']['timesRequested']}"
    response.write "\n\n"
    request.GET.each do |query_string|
      response.write "QueryString: #{query_string.first} = #{query_string.last}\n"
    end
    request.POST.each do |post_variable|
      response.write "POST Variable: #{post_variable.first} = #{post_variable.last}\n"
    end
    response.write "\n"
    puts env.to_yaml
    env.each {|key, value| response.write "Header: #{key} = #{value}\n" }
  when '/boom'
    response.write "Boom!"
    response.status = 500
  when '/headers'
    response.write 'This has custom headers FOO and BAR'
    response.headers['FOO'] = 'This is the value of foo'
    response.headers['BAR'] = 'Bar is different'
  when '/redirect'
    response.write 'Redirecting'
    response.headers['Location'] = '/info?redirected=true'
    response.status = 302
  else
    response.write "Not Found: #{verb} #{path}"
    response.status = 404
  end

  response.finish
}
