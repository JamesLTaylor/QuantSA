# ruby test.rb username password
# ftp all the files

require 'net/ftp'
puts "Starting"

username = ARGV[0]
password = ARGV[1]

ftp = Net::FTP.new(host='quantsa.org', username = username, password=password, passive=true)
ftp.chdir('public_html')
puts "set ftp folder"
files = Dir.glob("../Documentation/_site/*.*")
puts "get all files"
for f in files    
	puts "skipping  #{f} because FTP is disabled"
    # puts "putting #{f}"
    # ftp.puttextfile(f)
end
ftp.close

puts "Copied #{files.size} files"