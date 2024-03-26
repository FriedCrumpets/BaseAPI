using Newtonsoft.Json.Linq;

namespace RESTful_web_API_Course.Utils;

// todo: This needs significant testing. Not sure this will work at all...
public abstract class Patchable<T> {
    public abstract string Operation { get; }
    public string From { get; internal set; }
    public string Path { get; internal set; }
    public T? Value { get; set; }
}

public class AddPatch<T> : Patchable<T> {
    public override string Operation => "add";
}

public class RemovePatch<T> : Patchable<T> {
    public override string Operation => "remove";
}

public class ReplacePatch<T> : Patchable<T> {
    public override string Operation => "replace";
}

public class CopyPatch<T> : Patchable<T> {
    public override string Operation => "copy";
}

public class MovePatch<T> : Patchable<T> {
    public override string Operation => "move";
}

public class TestPatch<T> : Patchable<T> {
    public override string Operation => "test";
}

public static class PatchableExtensions {
    public static Patchable<T> Path<T>(this Patchable<T> patch, string path) {
        patch.Path += $"/{path}";
        return patch;
    }

    public static Patchable<T> From<T>(this Patchable<T> patch, string from) {
        patch.From += $"/{from}";
        return patch;
    }
    
    public static string Convert<T>(this Patchable<T> patch) {
        var obj = new JObject {
            { "op", JToken.FromObject(patch.Operation) },
            { "path", JToken.FromObject(patch.Path) }
        };
        
        if(null != patch.Value)
            obj.Add("value", JToken.FromObject(patch.Value));

        return obj.ToString();
    }
    
    public static string Convert<T>(this IEnumerable<Patchable<T>> patches) {
        return string.Join(',', patches.ConvertPatches());
    }

    private static IEnumerable<string> ConvertPatches<T>(this IEnumerable<Patchable<T>> patches) {
        return patches.Select(patch => patch.Convert());
    }
}