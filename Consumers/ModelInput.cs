using Microsoft.ML.Data;

namespace Endpoint;

public class ModelInput
{
    [ColumnName(@"Sentence")]
    public string Sentence { get; set; }

    [ColumnName(@"Label")]
    public float Label { get; set; }
}
