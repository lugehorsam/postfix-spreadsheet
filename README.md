## A Reverse Polish Notation evaluator with cell and letter number notation support.

### Running the Application

To develop or run this application, ensure you are on Mac OSX and
download Mono 5.10.1: http://www.mono-project.com/download/stable/

The most recent executable can be found in `PostfixCellEvaluator/bin/Debug`. Go to that directory,
create a file in CSV format (see tests for examples) and run the following:

`mono PostfixCellEvaluator <path-to-file>`

You should see the appropriate output printed in your console.

### Testing

From within `PostfixCellEvaluator/Tests/`, run the following:

`./run-tests.sh`

Ensure that all tests pass. Follow the format outlined in that script to add more tests.

### Implementation Note

The core parsing algorithm can be found in `PostfixCell.Evaluate()`

Future work on this method should ensure that it is readable and clearly separated
into discrete token operations (e.g., resolving cell links, handling operands, handling operators.)

Additionally, under less time constraints, look into breaking up its implementation
into smaller methods. Also audit a couple other methods for readability and size.
