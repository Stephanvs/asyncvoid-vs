asyncvoid-vs
============

Changing the world one async void return type at a time.

What is this?
=============

It's actually quite simple, this Visual Studio Extension detects the following code snippet:

```csharp
public async void SomeMethod()
{
    ...
}
```

and helps you to refactor it into

```csharp
public async Task SomeMethod()
{
    ...
}
```
