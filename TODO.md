- split `TypeReference.FullName` into `Namespace` and `Name`
- `CollectNamespaces(HashSet<string>)`
  - or `GetWrapped(SyntaxContext)`?

---

- make writing flags enums easier
  - use it for `MemberModifiers`
- consider creating code from "template" lambda or local function  
  - basically automatic code quoter
  - "lambda" parameters for inlining other code blocks
- write a tool that ensures no Roslyn types are exposed in the public interface
  - protected abstract methods will probably have to treated as an exception