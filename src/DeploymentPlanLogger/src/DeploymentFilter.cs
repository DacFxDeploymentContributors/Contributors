using System;
using System.Collections.Generic;
using Microsoft.SqlServer.Dac.Deployment;
using Microsoft.SqlServer.Dac.Extensibility;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace DeploymentPlanLogger
{
    [ExportDeploymentPlanModifier("AgileSqlClub.DeploymentPlanLogger", "0.1.0.0")]
    public class DeploymentFilter : DeploymentPlanModifier
    {
        private const int MaxDepth = 6;

        protected override void OnExecute(DeploymentPlanContributorContext context)
        {
            try
            {
                PublishMessage(new ExtensibilityError("Starting AgileSqlClub.DeploymentPlanLogger", Severity.Message));
               
                var next = context.PlanHandle.Head;
                while (next != null)
                {
                    var current = next;
                    next = current.Next;

                    DumpDeploymentStep(current);

                    if (current is CreateElementStep)
                        Console.WriteLine("here");

                    if (current.GetType().DeclaringType != null)
                    {
                        Console.WriteLine("\t");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void DumpDeploymentStep(object step, int depth = 0)
        {
            if (step == null || depth > MaxDepth)
                return;

            if (depth == 0)
            {
                Console.WriteLine("     **********************************\r\n" + step.GetType());
            }

            if (HandleCustom(step, depth))
                return;

            try
            {
                if (step is IEnumerable<object>)
                {
                    foreach (var o in (step as IEnumerable<object>))
                    {
                        foreach (var p in o.GetType().GetProperties())
                        {
                            Console.WriteLine("".PadRight(depth, '\t') + p.Name + " = " + p.GetValue(o) + " type: " +
                                              p.GetType());

                            if (p.Name != "Next" && p.Name != "Previous" && p.Name != "ScriptGenerator" &&
                                o.GetType() != typeof (string))
                                DumpDeploymentStep(p.GetValue(o), depth + 1);
                        }
                    }
                }
                else
                {
                    foreach (var p in step.GetType().GetProperties())
                    {
                        if (p.Name != "Next" && p.Name != "Previous" && p.Name != "ScriptGenerator" &&
                            !p.PropertyType.FullName.StartsWith("System.")
                            && p.Name != "StartOffset" && p.Name != "FragmentLength" && p.Name != "StartLine"
                            && p.Name != "StartColumn" && p.Name != "FirstTokenIndex" && p.Name != "LastTokenIndex"
                            && p.Name != "ScriptTokenStream")
                        {
                            Console.WriteLine("".PadRight(depth, '\t') + p.Name + " = " + p.GetValue(step));

                            DumpDeploymentStep(p.GetValue(step), depth + 1);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("".PadRight(depth, '\t') + "Error getting properties: " + e.Message + " step type: " +
                                  step.GetType());
            }
        }

        private bool HandleCustom(object step, int depth)
        {
            if (step is TSqlScript)
            {
                var script = step as TSqlScript;

                var generator = new Sql120ScriptGenerator();
                string scriptText;
                generator.GenerateScript(script, out scriptText);
                Console.WriteLine("".PadRight(depth, '\t') + " Script: " + scriptText);


                foreach (var batch in script.Batches)
                {
                    foreach (var statement in batch.Statements)
                    {
                        Console.WriteLine("".PadRight(depth, '\t') + statement.GetType());
                        DumpDeploymentStep(statement, depth + 1);
                    }
                }

                return true;
            }


            if (step is Identifier)
            {
                var id = step as Identifier;
                Console.WriteLine("".PadRight(depth, '\t') + id.Value);
                return true;
            }

            if (step is SchemaObjectName)
            {
                var id = step as SchemaObjectName;
                Console.WriteLine("".PadRight(depth, '\t') +
                                  string.Format("{0}.{1}.{2}.{3}",
                                      id.ServerIdentifier != null ? id.ServerIdentifier.Value : "",
                                      id.DatabaseIdentifier != null ? id.DatabaseIdentifier.Value : "",
                                      id.SchemaIdentifier != null ? id.SchemaIdentifier.Value : "",
                                      id.BaseIdentifier != null ? id.BaseIdentifier.Value : ""));
                return true;
            }
            return false;
        }
    }
}