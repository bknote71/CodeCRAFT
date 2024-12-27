protoc -I=./ --csharp_out=./ ./Protocol.proto
XCOPY ".\Protocol.cs" ".\CodeCRAFT" /Y

DEL Protocol.cs