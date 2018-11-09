# RxAction

An RxAction will do some work when executed with an input. While executing, zero or more output values and/or a failure may be generated.

RxActions are useful for performing side-effecting work upon user interaction, like when a button is clicked. Actions can also be automatically disabled based on a property, and this disabled state can be represented in a UI by disabling any controls associated with the action.

## How does it work

An RxAction's constructor takes a mandatory `workFactory` function that takes some input and produces an Observable. It also optionally takes an `enabledIf` Observable (defaults to true if not specified).

When `Execute` is called, the RxAction passes the parameter (if any) to the `workFactory` function and subscribes to the Observable it returns. So it's *creating a new Observable every time the action is executed.*

The Observable returned from the `workFactory` represents some work that needs to be done. The action will subscribe to that Observable and emit next events or errors as needed.

Regardless of the value sent as `enabledIf` the RxAction is disabled while it's being executed. That means you can't execute an action twice at the same time.

Calling the `Execute` method will perform the underlying action, *regardless of whether there are any subscriptions to it*. The fact that it's returning an Observable is just there for the convenience, in case we want to be informed of the state of the execution of the action, as it'll emit the elements the action returns and its potential errors.

## Public properties

`RxAction` exposes the following properties:

### Elements
`IObservable<TElement> Elements`

This is an observable that forwards the events resulting from the executed operation. No errors will show up here.

### Errors
`IObservable<Exception> Errors`

The errors of the execution, this includes any error generated in the Observable returned by `workFactory` and also any errors occurred in the action itself, which right now can only be `RxActionNotEnabledException`

### Executing
`IObservable<bool> Executing`

Sends `true` when excution starts and `false` when it ends.

### Enabled
`IObservable<bool> Enabled`

Wether or not the action is enabled. This is calculated from the `enabledIf` Observable and if the action is currently being executed.

### Inputs
`ISubject<TInput> Inputs`

This is the Subject that gets the execution started. Every event sent through it starts a new execution of the action (unless the action is already executing). Moreover, the `Execute` method calls `OnNext` on this Subject to start the execution. It's exposed publicly so we can easily bind an RxAction to buttons or other UI elements.

## Code samples

For example, this is an action that takes a string and returns a bool Observable.

```c#
var checkEmailExistAction = new RxAction<String, bool>(email => {
  return myAPI.checkEmail(email);
});
```
or shorter
```c#
var checkEmailExist = new RxAction<String, bool>(myAPI.checkEmail);
```
we would run it like this:
```c#
checkEmailExist.Execute("ricardo@toggl.com");
```
We could also specify an `enabledIf` parameter
```c#
Observable<bool> emailCheckingEnabled = ...
var checkEmailExist = new RxAction<String, bool>(checkEmail, emailCheckingEnabled);
```
If we try to run `checkEmailExist` while the action is disabled, it'll return an error. But we don't need to manually disable it while it's being executed, that gets handled automatically by the action.

Optionally we can subscribe to events generated by the action through the Observable returned by `Execute`:
```c#
checkEmailExist.Execute("ricardo@toggl.com")
  .Subscribe(
    exists => {
      Console.WriteLine($"Email exist: {exist}");
    },
    error => {
      Console.WriteLine($"Something went wrong: {error}");
    }
  )
  .DisposedBy(disposeBag);
```
Or through the properties exposed by the action, which would allow to do so when defining the action, instead of doing it when executing it.
```c#
var checkEmailExist = new RxAction<String, bool>(checkEmail);

checkEmailExist.Elements
  .Subscribe(
    exists => {
      Console.WriteLine($"Email exist: {exist}");
    }
  )
  .DisposedBy(disposeBag);

checkEmailExist.Errors
  .Subscribe(
    errors => {
      Console.WriteLine($"Something went wrong: {error}");
    }
  )
  .DisposedBy(disposeBag);
```

We can also use the `Executing` property to know when the action is being executed and show a loading indicator, for example.

```c#
checkEmailExist.Executing
  .Subscribe(isExecuting => {
    if (isExecuting) {
      showLoadingIndicator();
    } else {
      hideLoadingIndicator();
    }
  })
  .DisposedBy(disposeBag);
```

## Using it from the UI

We provide two subclasses to `RxAction` to make the most common uses more convenient, those are:

- `InputAction<TInput>` which is basically an `RxAction<TInput, Unit>`
- `UIAction` which is `RxAction<Unit, Unit>`

To bind the execution of one of these to a button tap we would do something like this

```c#
myButton.Rx().Tap()
    .Subscribe(myAction.Inputs)
    .DisposedBy(disposeBag);
```

But better yet, we could use one of the `BindAction` extensions on UIButton.

```c#
myButton.Rx()
    .BindAction(myAction)
    .DisposedBy(disposeBag);
```

This will also bing the enabled Observable back to the button. You can also specify the event that would trigger the action (tap by default). For example, for long press:

```c#
myButton.Rx()
    .BindAction(myAction, ButtonEventType.Longpress)
    .DisposedBy(disposeBag);
```

There's also a similar method for UIView and version for Android Views. It's recommended to write this extensions for other commonly used UI controls that are bound to an action and can be disabled.