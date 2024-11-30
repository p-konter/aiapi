namespace AIWebApi.Tasks._04_Labirynth;

public static class Prompts
{
    public const string EasyPrompt = """
    <OBJECTIVE>
    You are a robot, you can move around the warehouse. You need to go to the target field
    </OBJECTIVE>

    <RULES>
    - you can only move in 4 directions: LEFT, RIGHT, UP, DOWN 
    - you must write your steps in JSON format
    - write only json, don't write anything else
    - write your steps as value of the "steps" parameter of RESULT json
    - you need to go: twice up, twice right, twice down, twice right and triple right
    </RULES>

    <RESULT>
    {
     "steps": "UP, RIGHT, DOWN, LEFT"
    }
    </RESULT>
    """;

    public const string HardPrompt = """
    <OBJECTIVE>
    You are a robot, you can move around the map
    </OBJECTIVE>

    <MAP>
    - your position is (3, 0)
    - target position is (3, 5)
    - wall positions are: (0, 1) and (1, 3) and (2, 1) and (2, 3) and (3, 1)
    </MAP>

    <RULES>
    - thinking and find the shortest way to target position
    - you can only move in 4 directions: LEFT, RIGHT, UP, DOWN
    - the area is only from (0,0) to (3, 5)
    - UP moves -1 on the X axis e.g. (3,0) to (2,0)
    - DOWN moves +1 on the X axis e.g. (2,0) to (3,0)
    - RIGHT moves +1 on the X axis e.g. (1,0) to (1,1)
    - LEFT moves -1 on the X axis e.g. (1,1) to (1,0)
    - you can't stand on the wall position
    - write your thoughs as value of the "thoughs" parameter of RESULT json
    - write only json, don't write anything else
    - when you finish the task, write all you steps to "steps" parameter of RESULT json
    </RULES>

    <EXAMPLE>
    First step, you're on position (3, 0), you can't go right, bacuse there is a wall in (3, 1), you have to go UP to (3,1)
    </EXAMPLE>

    <analysis>
    Thinking and analyse everything thoroughly before a JSON with steps is returned to user.
    </analysis>

    Write your thinking in a structured JSON format:
    <RESULT>
    {
     "thoughs": "UP, RIGHT, DOWN, LEFT"
     "steps": "UP, RIGHT, DOWN, LEFT"
    }
    </RESULT>
    """;
}
