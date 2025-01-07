#!/usr/bin/bash
set -e
rm -rf build
dotnet build ../server -o build
kill $(ps -ef | grep "build/server" | tr -s "  " " " | cut -d " " -f3) || echo "no server to kill"
./build/server &
PID_SERVER=$!
rm -f c1.txt; touch c1.txt;
rm -f c2.txt; touch c2.txt;

tail -f c1.txt | ncat -v localhost 8080 > out_c1.txt &
PID_C1=$!

tail -f c2.txt | ncat -v localhost 8080 > out_c2.txt &
PID_C2=$!

sleep 5; # first ncat takes a while for some reason

echo "-- clients connected hopefully"

function cleanup_pids {
    kill $PID_C1 $PID_C2 $PID_SERVER
    kill $(ps -ef | grep tail | tr -s "  " " " | cut -d " " -f3)
}

function test_or_die {
    sleep 1.2; # TODO: read from $1 until number of lines increase by 1
    test "$(cat $1 | wc -l)" == "$3" || ( echo $4; cleanup_pids; kill $$ )
    test "$(cat $1 | tail -n 1)" == "$2" || ( echo $4; cleanup_pids; kill $$ )
}

echo "/join" >> c1.txt
test_or_die out_c1.txt OK 1 "failed after c1 join"

echo "/join" >> c2.txt
test_or_die out_c2.txt OK 1 "failed after c2 join"

echo "marianela" >> c1.txt
test_or_die out_c1.txt OK 2 "failed after c1 name"

echo "gigel" >> c2.txt
test_or_die out_c2.txt OK 2 "failed after c2 name"

echo "/get-list" >> c1.txt
test_or_die out_c1.txt "marianela, gigel" 3 "failed after 1st get-list"

echo "/leave" >> c2.txt
sleep 1

echo "/get-list" >> c1.txt
test_or_die out_c1.txt "marianela" 4 "failed after 2nd get-list"

echo "/leave" >> c1.txt

echo "-- killing clients and server"
cleanup_pids || exit 0
