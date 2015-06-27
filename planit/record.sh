ffmpeg -video_size 580X425 -show_region 1 -framerate 30 -f x11grab -i :0.0+85,50 -c:v libx264 -qp 0 -preset ultrafast $1
