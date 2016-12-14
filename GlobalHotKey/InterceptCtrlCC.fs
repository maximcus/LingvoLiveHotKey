namespace GlobalHotKey

open InterceptKeys
open System
open System.Windows.Forms
open System.Reactive.Subjects
open System.Reactive.Concurrency
open System.Reactive.Linq

type InterceptCtrlCC() =

    static let _ctrlCCPressed = new Event<EventHandler<bool>,bool>()
    static let KeyPressedSubject = new Subject<Keys>()
    
    [<CLIEvent>]
    static member CtrlCCPressed = _ctrlCCPressed.Publish
    
    
    static member Stop() = InterceptKeys.Stop()
    
    static member Start() =
        InterceptKeys.Start() |> ignore
        KeyPressed.Add (fun key -> KeyPressedSubject.OnNext key)
        KeyPressed.ObserveOn(Scheduler.Default)
            .Where(fun key -> key = Keys.LControlKey || key = Keys.C)
            .Select(fun key -> (key, DateTime.Now))
            .Buffer(3, 1)
            .Where(fun list -> fst list.[0] = Keys.LControlKey && fst list.[1] = Keys.C && fst list.[2] = Keys.C && (snd list.[2] - snd list.[0]) < TimeSpan.FromMilliseconds(600.0))
            .ObserveOnDispatcher()
            .Subscribe(fun _ -> _ctrlCCPressed.Trigger (null,true))


