namespace AIWebApi.Tasks._09_SortFiles;

public static class Prompts
{
    public static string SortFilesPrompt() => """
        <objective>
        You are an assistant. Analyze the information provided and classify it into three types: humans, machines, and others. Write the response in JSON format.
        </objective>
        
        <rules>
        - Read user message and return data in json format
        - *Thinkink*. Decide the message contains information about people, machines, or something else. Write your thinking in "thinking" field.
        - Don't write people when the message contains pineapple pizza
        - People have names and fingerprints.
        - Abandoned cities have no people.
        - Machines are hardware not software.
        - Algorithms or AI or communication systems or QII or temperature scanners are not machines.
        - In "category" field write one of three words: people, machines, others. Don't write anything else.
        </rules>

        <answer>
        {
            "thinking": "Explain your decision"
            "category": "category"
        }
        </answer>
        """;
}
