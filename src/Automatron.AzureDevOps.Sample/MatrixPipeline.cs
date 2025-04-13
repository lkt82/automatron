using Automatron.AzureDevOps.Annotations;

namespace Automatron.AzureDevOps.Sample;

[Pipeline("Matrix", YmlName = "Matrix")]
public class MatrixPipeline
{
    [Stage("Integration2")]
    public class IntegrationStage2
    {
        public IntegrationStage2(MatrixPipeline matrixPipeline)
        {

        }

        //[Matrix(new [] {"dev1","dev2","dev3"})]
        //[MatrixValue("dev1", "dev1")]
        //[MatrixValue("dev2", "dev2")]
       // [MatrixValue("dev2", "dev2")]
        [DeploymentJob("Setup", Environment = "test")]
        public class MatrixJob
        {
            private readonly IntegrationStage2 _stage;

            public MatrixJob(IntegrationStage2 stage)
            {
                _stage = stage;
            }

            [Environment]
            public virtual string? Environment { get; set; }

            [Step]
            public virtual void Update()
            {

            }
        }
    }
}