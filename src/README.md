# Folder Names

- "config" folder, for classes related to config file management (no resource instances, etc.)
- "data" folder, Readonly structs/records, NOT resources. This is for data that can't be represented in resource format
- "enums" folder, self-explanatory
- "exceptions" folder, self-explanatory
- "helpers" folder, static classes: should not hold changing state, allows for extension methods, should be named with Utility suffix.
- "scripts" folder, anything that gets assigned to a GodotObject (resources, nodes)
- "shaders" folder, self-explanatory, no materials
- "single" folder, for scripts that act as services, where there is only one instance expected at a time (they are injected by Game)
- "valueobjects" folder, for Vogen valueobject definitions (DDD)
