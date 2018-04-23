#!/bin/bash

# Runs tests on *.csv files in its working directory. Each test is represented by an input
# and output file, identified by the INPUT_SUFFIX and OUTPUT_SUFFIX.
# Tests pass if the program generates the content in the output file from the 
# content in the input file.

INPUT_SUFFIX="_Input"
OUTPUT_SUFFIX="_Output"

EXE_PATH="$(dirname $PWD)/bin/Debug/PostfixCellEvaluator.exe"

echo ""
echo "=================="
echo "Running Test Suite"
echo "=================="

for input_file in $PWD/*$INPUT_SUFFIX.csv
  do
    file_base=$(basename $input_file)
    file_prefix="${file_base%_*}"
    echo ""
    echo "Running test for: $file_prefix..."
    
    output_file=$file_prefix$OUTPUT_SUFFIX.csv
      
    if [ ! -f $output_file ]; then
      echo "Output file not found. Please ensure there is an output file for each test's input file."
      exit 1
    fi
    
    input_file_content=$(cat $input_file)
    output_file_content=$(cat $output_file)
    program_output=$(mono $EXE_PATH $input_file)
    
    if [ "$program_output" = "$output_file_content" ]; then
      echo "Test Passed"
    else
      echo ""
      echo "TEST FAILED"
      echo ""
      echo Expected: 
      echo "$output_file_content"
  
      echo ""
      echo Actual: 
      echo "$program_output"
    fi
done

echo ""
