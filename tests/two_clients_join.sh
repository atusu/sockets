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

sleep 3; # first ncat takes a while for some reason

echo "-- clients connected hopefully"

function cleanup_pids {
    kill $PID_C1 $PID_C2 $PID_SERVER
    kill $(ps -ef | grep tail | tr -s "  " " " | cut -d " " -f3) || echo "no such process"
}

function wait_n_lines {
    n_tries=0
    while true; do
        n_lines=$(cat $1 | wc -l)
        n_tries=$((n_tries+1))
        if [ $n_lines -lt "$2" ]; then
            sleep 0.01
            continue
        else
            break
        fi
        if [ $n_tries == "1000" ]; then # stop after ~10s of waiting
            break
        fi
    done
    echo "waited $n_tries tries"
}

function test_or_die {
    wait_n_lines $1 $3
    test "$(cat $1 | tail -n 1)" == "$2" || ( echo $4; echo "in file $1:"; cat $1; cleanup_pids; kill $$ )
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
wait_n_lines out_c1.txt 3
test "$(cat out_c1.txt | tail -n 1)" == "marianela, gigel" || \
    test "$(cat out_c1.txt | tail -n 1)" == "gigel, marianela" || \
    ( echo $4; echo "in file:"; cat out_c1.txt; cleanup_pids; kill $$ )

echo "/leave" >> c2.txt
sleep 1

echo "/get-list" >> c1.txt
test_or_die out_c1.txt "marianela" 4 "failed after 2nd get-list"

echo "/leave" >> c1.txt

echo "-- killing clients and server"
cleanup_pids
