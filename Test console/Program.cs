// See https://aka.ms/new-console-template for more information
using HUREL.PG;
using HUREL.PG.Ncc;




Console.WriteLine("Hello, World!");
Console.WriteLine();

NccSession session = new NccSession();
string planFileDir = @"D:\OneDrive - 한양대학교\01. Research\01. 통합 제어 프로그램\99. FunctionTest\02. (완료) Shift and merge 디버깅\검증 자료\01. Plan\3DplotMultiSBox5Gy.pld";
session.LoadPlanFile(planFileDir);
