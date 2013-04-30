REM This is a windows batch file test

@ECHO off

ECHO This is a batch file test!
>>test.txt ECHO This is getting written to test.txt
ECHO This is getting written to stdout

IF DEFINED PLEASEFAIL (EXIT 1)

EXIT 0
