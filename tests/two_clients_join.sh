#!/usr/bin/bash
rm -rf build
dotnet build ../server -o build
kill $(ps -ef | grep "build/server" | tr -s "  " " " | cut -d " " -f3)
./build/server.exe &
PID_SERVER=$!
rm -f c1.txt; touch c1.txt;
rm -f c2.txt; touch c2.txt;

tail -f c1.txt | ncat.exe -v localhost 8080 > out_c1.txt &
PID_C1=$!

tail -f c2.txt | ncat.exe -v localhost 8080 > out_c2.txt &
PID_C2=$!

sleep 5;

echo "-- clients connected hopefully"

function echo_sleep {
    echo $1 >> $2
    sleep 1
}

function test_or_die {
    test $(cat $1 | wc -l) == "$3" || ( echo $4; kill $PID_C1 $PID_C2 $PID_SERVER; kill $$ )
    test "$(cat $1 | tail -n 1)" == "$2" || ( echo $4; kill $PID_C1 $PID_C2 $PID_SERVER; kill $$ )
}

echo_sleep "/join" c1.txt
test_or_die out_c1.txt OK 1 "failed after c1 join"

echo_sleep "/join" c2.txt
test_or_die out_c2.txt OK 1 "failed after c2 join"

echo_sleep "marianela" c1.txt
test_or_die out_c1.txt OK 2 "failed after c1 name"

echo_sleep "gigel" c2.txt
test_or_die out_c2.txt OK 2 "failed after c2 name"

echo_sleep "/get-list" c1.txt
test_or_die out_c1.txt "marianela, gigel" 3 "failed after 1st get-list"

echo_sleep "/leave" c2.txt

echo_sleep "/get-list" c1.txt
test_or_die out_c1.txt "marianela" 4 "failed after 1st get-list"

echo_sleep "/leave" c1.txt

echo "-- killing clients and server"
kill $PID_C1 $PID_C2 $PID_SERVER
