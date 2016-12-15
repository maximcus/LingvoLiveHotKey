namespace GlobalHotKey

open InterceptKeys
open System
open System.Reactive.Concurrency
open System.Reactive.Linq
open System.Windows.Forms

type InterceptCtrlCC() =

    static let _ctrlCCPressed = new Event<EventHandler<bool>,bool>()
    
    [<CLIEvent>]
    static member CtrlCCPressed = _ctrlCCPressed.Publish    
    
    static member Stop() = InterceptKeys.Stop()
    
    static member Start() =
        InterceptKeys.Start() |> ignore
        KeyPressed.ObserveOn(Scheduler.Default)
            .Where(fun key -> key = Keys.LControlKey || key = Keys.C)
            .Select(fun key -> (key, DateTime.Now))
            .Buffer(3, 1)
            .Where(fun list -> fst list.[0] = Keys.LControlKey && fst list.[1] = Keys.C && fst list.[2] = Keys.C && (snd list.[2] - snd list.[0]) < TimeSpan.FromMilliseconds(600.0))
            .ObserveOnDispatcher()
            .Subscribe(fun _ -> _ctrlCCPressed.Trigger (null,true))


