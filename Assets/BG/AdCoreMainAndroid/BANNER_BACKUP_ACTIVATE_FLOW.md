# Banner/Mrec Rect Fallback Flow

Tài liệu này mô tả đúng logic runtime hiện tại của:
- `BannerFallbackGroup`
- `MrecFallbackGroup`

Cả hai đang dùng chung lõi:
- [RectFallbackGroupBase.cs](/d:/Unity%20projects/BG-Library-Full-Module/Assets/BG_Lib/AdCore-MainForAndroid/RectFallbackGroupBase.cs)

Khác biệt duy nhất:
- `Mrec` có thêm phần giữ và apply lại position
- `Banner` chỉ dùng đúng flow rect fallback

## 1. Mục tiêu

Logic rect fallback hiện tại nhằm đạt 4 mục tiêu:

1. luôn cố giữ 1 candidate đang show nếu đã có ad
2. candidate đang visible không bị timeout-destroy/rebuild
3. candidate hidden mới là bên được nuôi lại ở nền
4. impression chỉ lấy từ callback thật của mediation

## 2. Candidate Và Thứ Tự

Mỗi rect placement có tối đa 2 candidate:
- `Admob`
- `Max`

Thứ tự candidate được build ở [AdCore_MainAndroid.cs](/d:/Unity%20projects/BG-Library-Full-Module/Assets/BG_Lib/AdCore-MainForAndroid/AdCore_MainAndroid.cs).

Rule:
- `useBackup = false`
  - chỉ build đúng `priority`
- `useBackup = true`
  - build `priority` trước
  - sau đó mới thêm mediation còn lại

Nên:
- `A0` thường là candidate đầu tiên
- `M1` thường là candidate thứ hai

## 3. State Quan Trọng

Mỗi placement rect có các state chính:

- `currentCandidateIndex`
  - candidate đang được coi là owner hiện tại
- `isViewActive`
  - placement hiện có đang được `ActivateView()` hay không
- `lastVisibleCommandMode`
  - lần hiện gần nhất là `Activate` hay `Show`
- `lastVisiblePos`
  - vị trí/pos cuối cùng được dùng để present

Mỗi candidate có runtime state:

- `HasEverLoaded`
  - đã từng `load success` ít nhất 1 lần chưa
- `HasImpressionSinceLastLoad`
  - từ lần load success gần nhất đã có `OnPaid` chưa
- `IsRebuilding`
  - đang trong pha rebuild
- `AwaitingRecovery`
  - đang được watchdog theo dõi recovery
- `RefreshFailStreak`
  - số lần `refresh fail` liên tiếp của candidate đang visible
- `RecoveryFailCount`
  - số lần hidden recovery fail dùng để tăng timeout nền

## 4. Fresh / Stale / Usable

Trong runtime hiện tại:

- `usable`
  - candidate đã `WasInitialized`
  - và `Group.IsLoaded == true`

- `fresh`
  - candidate `usable`
  - và `HasImpressionSinceLastLoad == false`

- `stale`
  - candidate `usable`
  - nhưng đã có `OnPaid` kể từ lần load gần nhất

Current implementation chỉ swap lên candidate `fresh`.

## 5. Initialize

`Initialize()` chỉ init candidate đầu tiên chưa init.

Flow:
1. init candidate đầu
2. bật recovery watch cho candidate đó
3. nếu candidate đó `initial fail`
4. hệ init candidate kế tiếp

Nên:
- `initial fail` mới mở candidate tiếp theo
- không phải cứ mọi fail là tự init candidate tiếp

## 6. ActivateView Lần Đầu

Khi `ActivateView()` được gọi:

1. `isViewActive = true`
2. lưu `lastVisiblePos`
3. lưu `lastVisibleCommandMode = Activate`

Sau đó:

- nếu current candidate đang usable
  - present lại current
- nếu chưa có current usable nhưng có candidate `fresh`
  - chọn candidate `fresh` đầu tiên
- nếu chưa có `fresh` nhưng có candidate usable
  - chọn candidate usable đầu tiên
- nếu chưa có ai usable
  - không show ngay
  - chỉ giữ visible intent và chờ `Loaded`

Đây là rule rất quan trọng:
- `ActivateView()` dù lúc đó chưa có ad
- hệ vẫn nhớ placement đang active
- candidate nào load được sau đó sẽ có quyền lên

## 7. Hide

`Hide()` chỉ làm:

1. `isViewActive = false`
2. hide current candidate nếu có

Nó không làm:
- repick candidate
- swap candidate
- destroy current candidate

## 8. Activate Lại Sau Hide

Khi `ActivateView()` được gọi lại sau một lần `Hide()`:

- nếu current candidate còn usable
  - show/activate lại chính nó
- nếu current không còn usable
  - quay về logic activate lần đầu

Tức là:
- hide rồi activate lại không phải lúc để đổi owner
- trừ khi current không còn usable nữa

## 9. Khi Candidate Loaded

Trong `HandleRectLoaded(...)`:

1. `HasEverLoaded = true`
2. `HasImpressionSinceLastLoad = false`
3. `RefreshFailStreak = 0`
4. `RecoveryFailCount = 0`
5. kết thúc watchdog của candidate đó

Sau đó:

- nếu placement chưa active
  - không show gì cả
- nếu placement đang active và chưa có current usable
  - chọn candidate tốt nhất rồi present
- nếu placement đang active và candidate vừa loaded không phải current
  - gọi `TrySwapCurrentWithBackup("backup_loaded")`

## 10. Khi Candidate Initial Fail

Trong `HandleRectLoadFailed(...)`, nếu candidate chưa từng load success:

1. tăng `RecoveryFailCount`
2. `WatchTouch`
3. log `IF`
4. nếu đây là candidate initialized cao nhất hiện tại
  - init candidate tiếp theo

Nghĩa là:
- `initial fail` là tín hiệu mở candidate tiếp
- đồng thời cũng là tín hiệu làm timeout nền tăng dần

## 11. Khi Candidate Refresh Fail

Nếu candidate đã từng load success:

1. tăng `RefreshFailStreak`
2. nếu candidate này không phải current hoặc placement không active
  - coi như hidden fail
  - tăng `RecoveryFailCount`
  - `WatchTouch`
3. log `RF`

Nếu candidate fail này là current visible:

1. ping backup
2. thử swap ngay

Nếu candidate fail này không phải current visible:
- không swap
- chỉ cập nhật recovery state nền

## 12. Ping Backup

Current candidate khi `refresh fail` sẽ ping backup candidate.

Backup phản ứng như sau:

- nếu đang `IsRebuilding`
  - bỏ qua ping
- nếu đã `fresh`
  - chỉ nhận thông báo, không rebuild lại
- nếu đang `AwaitingRecovery`
  - bỏ qua ping
- còn lại
  - `TryRebuildCandidate(...)`

Tức là backup không bị reset vô ích nếu nó đã ở trạng thái tốt.

## 13. Rebuild Candidate

`TryRebuildCandidate(...)` hiện làm:

1. reset:
  - `HasEverLoaded = false`
  - `HasImpressionSinceLastLoad = false`
  - `RefreshFailStreak = 0`
2. giữ `RecoveryFailCount` để timeout backoff không bị mất
3. mark `WasInitialized = true`
4. set tracking channel
5. gọi hook `OnCandidateInitialized(...)`
6. bật recovery watch
7. gọi `candidate.Group.Rebuild()`

Nếu `Rebuild()` trả fail ngay:
- tăng `RecoveryFailCount`
- dừng watch
- log `RBX`

## 14. Swap Rule

`TrySwapCurrentWithBackup(...)` chỉ swap nếu:

1. placement đang active
2. backup candidate là `fresh`
3. current không usable
   - hoặc `RefreshFailStreak >= 1`

Nghĩa là ngưỡng swap hiện tại là:
- `Admob = 1`
- `Max = 1`

Không còn rule cũ `2 / 3` nữa.

Nếu swap thành công:

1. hide previous current
2. kết thúc recovery watch của current cũ với reason `swapped_down`
3. present backup
4. current mới trở thành backup candidate vừa được chọn

Nếu swap fail:
- restore lại current cũ

## 15. Candidate Bị Swap Xuống

Candidate cũ sau khi bị swap xuống:

- chỉ bị `hide`
- không tự destroy
- không tự rebuild ngay

Nó sẽ chờ:
- ping sau từ current mới
- hoặc hidden recovery logic khác

Đây là rule đã được chốt rõ trong runtime hiện tại.

## 16. Watchdog Timeout

Watchdog chỉ áp dụng cho candidate đang:
- `AwaitingRecovery = true`

Nhưng có một rule cực quan trọng:

- nếu candidate đó là current visible
  - watchdog **không** destroy/rebuild nó
  - chỉ kết thúc watch với reason `timeout_ignore_visible_current`

Tức là:
- current đang show không bị timeout phá ad
- hidden candidate mới là bên bị timeout rồi rebuild

## 17. Backoff Timeout Hiện Tại

Timeout nền hiện tại:

- base: `20s`
- mỗi lần hidden recovery fail: `+5s`
- cap tối đa: `40s`

Formula:

```text
timeout = min(20 + RecoveryFailCount * 5, 40)
```

Ví dụ:

- `rf=0 -> to=20`
- `rf=1 -> to=25`
- `rf=2 -> to=30`
- `rf=3 -> to=35`
- `rf>=4 -> to=40`

Điều này giúp:
- hidden candidate quá yếu không bị rebuild quá dày
- nhưng vẫn tiếp tục được nuôi lại ở nền

## 18. Watchdog Có Reset Không?

Có.

Nếu candidate đang `AwaitingRecovery` mà mediation tự trả:
- `Loaded`

thì watchdog của candidate đó sẽ được kết thúc ngay.

Ngoài ra:
- nếu candidate đang await recovery mà vẫn tiếp tục trả fail callback
- hệ sẽ `WatchTouch`
- nghĩa là timeout sẽ được dời ra theo fail callback mới nhất

## 19. Impression

Impression vẫn chốt cứng theo callback thật:

- `OnRectPaid`
- hay callback revenue thật từ mediation

Runtime hiện tại không dùng:
- local timer
- visible duration tự suy luận

Khi `Paid` xảy ra:
- `HasImpressionSinceLastLoad = true`
- candidate từ `fresh` thành `stale`

## 20. Max Impression

Runtime hiện tại vẫn giữ patch:
- `CreateBanner/CreateMRec` của `Max`
- gọi `Hide...` ngay sau create

Mục tiêu:
- giảm nguy cơ `Max` ăn impression sớm trước khi wrapper quyết định show

Theo các log test gần nhất:
- chưa còn thấy lại pattern xấu ban đầu rõ như trước
- nên logic hiện tại đang được coi là chấp nhận được để chạy tiếp

## 21. Mrec Khác Banner Ở Đâu?

`MrecFallbackGroup` chỉ thêm một lớp mỏng:
- nhớ `positionMode`
- nhớ `lastAdPosition`
- nhớ `lastAnchorTarget`
- nhớ `lastAnchorCamera`

Mỗi lần candidate mới được init/rebuild:
- hook `OnCandidateInitialized(...)`
- apply lại position cho candidate đó

Ngoài phần này ra:
- `Mrec` và `Banner` dùng cùng state machine

## 22. Debug Flow Hiện Tại

Filter:
- `RectFB`

Các token chính:

- `o=BN.FB`
  - Banner FullBottom
- `o=BN.FT`
  - Banner FullTop
- `o=MR`
  - Mrec

- `c=A0 / M1`
  - current candidate
- `t=A0 / M1`
  - candidate mà dòng log đang nói tới
- `a=1 / 0`
  - placement đang active hay không
- `rf=...`
  - hidden recovery fail count
- `to=...`
  - timeout hiện tại của candidate đó

Action token:

- `WS`
  - WatchStart
- `WT`
  - WatchTouch
- `WD`
  - WatchDone
- `IF`
  - InitialFail
- `RF`
  - RefreshFail
- `RB`
  - Rebuild / PrepareBackup
- `RBX`
  - Rebuild fail ngay
- `TO`
  - RecoveryTimeout
- `PR`
  - Present
- `SW`
  - TrySwap
- `Ld`
  - Loaded
- `Pd`
  - Paid
- `PG`
  - Ping backup

## 23. Chốt Logic Runtime Hiện Tại

Runtime rect fallback hiện tại có thể tóm gọn như sau:

1. `ActivateView()` nhớ visible intent
2. chưa có ad thì chờ load
3. current visible fail thì ping backup
4. backup fresh thì có quyền swap lên
5. current visible không bị timeout phá
6. hidden candidate mới bị rebuild theo watchdog
7. hidden candidate càng fail nhiều thì timeout càng giãn
8. candidate bị swap xuống chỉ hide, chờ ping sau
9. `useBackup=false` thì chỉ dùng priority
10. impression chỉ từ callback thật
