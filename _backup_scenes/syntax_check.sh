#!/bin/bash
# 用团结引擎自带的 Roslyn 编译一遍 Assets 下所有 .cs，检查有没有真错误。
# 用法: syntax_check.sh <输出日志>   必须在项目根目录执行。
#
# 所有参数都写进响应文件：引用程序集有 250 多个，直接放命令行会 Argument list too long。
# 响应文件里只用 ASCII 相对路径（Assets/...），绕开中文路径的编码问题。

E="F:/2022.3.48t7/Editor/Data"
RSP="_backup_scenes/_rsp.rsp"
OUT="$1"

{
  echo "-nologo"
  echo "-preferreduilang:en-US"
  echo "-langversion:9.0"
  echo "-t:library"
  echo "-nowarn:1701,1702,0169,0649"
  echo "-out:_backup_scenes/_check.dll"

  for f in "$E/TuanjieReferenceAssemblies/tuanjie-4.8-api"/*.dll; do echo "-r:$f"; done
  for f in "$E/TuanjieReferenceAssemblies/tuanjie-4.8-api/Facades"/*.dll; do echo "-r:$f"; done
  # 这个目录里 UnityEngine.* 和 UnityEditor.* 的模块都有，别再单独引 UnityEditor.dll 门面，会 CS0433 重复
  for f in "$E/Managed/UnityEngine"/*.dll; do echo "-r:$f"; done
  # ScriptAssemblies 里只挑 UI 和 TMP，Assembly-CSharp.dll 必须排除，否则跟源码重复定义
  echo "-r:Library/ScriptAssemblies/UnityEngine.UI.dll"
  echo "-r:Library/ScriptAssemblies/Unity.TextMeshPro.dll"
  echo "-r:Library/PackageCache/com.unity.nuget.newtonsoft-json@3.2.2/Runtime/Newtonsoft.Json.dll"

  find Assets -name "*.cs"
} > "$RSP"

rm -f _backup_scenes/_check.dll
dotnet "$E/DotNetSdkRoslyn/csc.dll" "@$RSP" > "$OUT" 2>&1

echo "产物: $(ls _backup_scenes/_check.dll 2>/dev/null || echo '没生成 —— 编译没跑起来')"
echo "=== 错误码直方图 ==="
grep -oE 'error CS[0-9]+' "$OUT" | sort | uniq -c | sort -rn
echo "=== 总错误数 ==="
grep -cE 'error CS[0-9]+' "$OUT"
