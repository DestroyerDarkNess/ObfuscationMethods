﻿using dnlib.DotNet.Emit;
using dnlib.DotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protector.Protections
{
    internal class StringProxy
    {
        public static void Execute(ModuleDef module)
        {
            foreach (var type in module.GetTypes())
            {
                if (type.IsGlobalModuleType) continue;
                foreach (var meth in type.Methods)
                {
                    if (!meth.HasBody) continue;
                    var instr = meth.Body.Instructions;
                    foreach (var t in instr)
                    {
                        if (t.OpCode != OpCodes.Ldstr) continue;
                        var methImplFlags = MethodImplAttributes.IL | MethodImplAttributes.Managed;
                        var methFlags = MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.ReuseSlot;
                        var meth1 = new MethodDefUser(Renamer.GenerateName(),
                            MethodSig.CreateStatic(module.CorLibTypes.String),
                            methImplFlags, methFlags);
                        module.GlobalType.Methods.Add(meth1);
                        meth1.Body = new CilBody();
                        meth1.Body.Variables.Add(new Local(module.CorLibTypes.String));
                        meth1.Body.Instructions.Add(Instruction.Create(OpCodes.Ldstr, t.Operand.ToString()));
                        meth1.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));

                        t.OpCode = OpCodes.Call;
                        t.Operand = meth1;
                    }
                }
            }
        }
    }
}
