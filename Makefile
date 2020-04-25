all: pack

pack: build
	mkdir -p `pwd`/packages
	dotnet pack -c Release `pwd`/src/ENode.Eventual2PC/
	mv `pwd`/src/ENode.Eventual2PC/bin/Release/*.nupkg `pwd`/packages/

build:
	dotnet build -c Release `pwd`/src/ENode.Eventual2PC/
