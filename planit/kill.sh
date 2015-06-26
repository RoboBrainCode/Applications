disp=`ps x | grep ffmpeg`
disp1=`echo $disp | cut -d " " -f 1`
kill -INT $disp1

