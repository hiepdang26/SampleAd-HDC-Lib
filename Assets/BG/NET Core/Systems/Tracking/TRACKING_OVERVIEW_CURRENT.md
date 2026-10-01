# Tracking Overview Current

File này là bản tra nhanh theo code hiện tại của:

- [NetTrackingSystem.cs](/d:/Unity%20projects/BG-Library-Full-Module/Assets/BG_Lib/NetCore/Systems/Tracking/NetTrackingSystem.cs)
- [FS_GroupControllerBase.cs](/d:/Unity%20projects/BG-Library-Full-Module/Assets/BG_Lib/NetCore/Mediation%20Scripts/Controller/FS_GroupControllerBase.cs)
- [Rect_GroupControllerBase.cs](/d:/Unity%20projects/BG-Library-Full-Module/Assets/BG_Lib/NetCore/Mediation%20Scripts/Controller/Rect_GroupControllerBase.cs)

Nếu cần bản đầy đủ hơn, xem thêm [TRACKING_MATRIX.md](/d:/Unity%20projects/BG-Library-Full-Module/Assets/BG_Lib/NetCore/Systems/Tracking/TRACKING_MATRIX.md).

## 1. Hai họ event chính

### Command

```text
ad_{layer}_{channel?}_{action}_{id?}_{context?}_{target?}_n_{reason?}_api
```

Ví dụ:

- `ad_sy_bn_rq_fb`
- `ad_gr_bn_rq_bn720jy_ini_fb_api`
- `ad_gr_fa_rq_fa1234_ini_gameplay_api`
- `ad_sy_rw_sh_x2coin_n_cfg`

### Callback

```text
ad_evt_{result}_{action}_{id}_{context?}_{target?}_{tail...}
```

Ví dụ:

- `ad_evt_ls_rq_bn720jy_ini_fb_f`
- `ad_evt_lf_rq_rw1234_rt1_ec3_net0_f`
- `ad_evt_imp_sh_bn720jy_fb`
- `ad_evt_rwd_sh_rw1234_rewarded`

## 2. Token quan trọng

### Channel

- `fa` = ForceAd
- `rw` = Rewarded
- `ao` = AppOpen
- `bn` = Banner
- `mr` = Mrec
- `pu` = Popup
- `cl` = Collap

### Group ad type token

- `fa` = ForceAd
- `rw` = Rewarded
- `ao` = AppOpen
- `bn` = Banner
- `mr` = Mrec
- `pu` = Popup
- `cl` = Collap

### Context

- `ini` = Init
- `rt1`, `rt2`, ... = Retry
- `idl` = Idle self-heal
- `hdr` = Hide reload
- `sfr` = Display-fail reload

### Callback result

- `ls` = load success
- `lf` = load fail
- `imp` = impression
- `clk` = click
- `dsp` = displayed
- `cls` = closed
- `rwd` = rewarded
- `shf` = show failed

### Speed

- `f` = fast
- `n` = normal
- `s` = slow

Lưu ý:

- FS load fail có speed token.
- Rect load fail hiện dùng `LoadGroupFailNoSpeed(...)`, nên thường không có speed token ở cuối.

## 3. Identity / target rule

### ForceAd / Popup

Identity source có dạng:

```text
GroupName|AdUnitId
```

Nên:

- identity token = `fa{id}` hoặc `pu{id}`
- target token = `groupName`

Ví dụ:

- `fa1234` = ForceAd id rút gọn
- `gameplay` = group name token

### Rewarded / AppOpen / Banner / Mrec / Collap

Identity source chủ yếu là ad unit id.

Nên:

- có identity token
- thường không có target group name
- riêng rect có thể có target là `pos`

## 4. FS tracking hiện tại

FS load callback được bắn ở:

- [FS_GroupControllerBase.cs](/d:/Unity%20projects/BG-Library-Full-Module/Assets/BG_Lib/NetCore/Mediation%20Scripts/Controller/FS_GroupControllerBase.cs)
  - `OnAdLoadedEvent(...)`
  - `OnAdLoadFailedEvent(...)`

### ForceAd request API

Ví dụ:

```text
ad_gr_fa_rq_fa1234_ini_gameplay_api
```

Ý nghĩa:

- group layer
- channel `fa`
- request
- group identity `fa1234`
- context `ini`
- target `gameplay`
- reached api

### ForceAd load fail

Ví dụ:

```text
ad_evt_lf_rq_fa1234_ini_gameplay_ec3_net0_f
```

Ý nghĩa:

- callback load fail
- request flow
- id `fa1234`
- context `ini`
- target `gameplay`
- error code `3`
- offline
- load speed fast

### Rewarded request API

Ví dụ:

```text
ad_gr_rw_rq_rw1234_ini_api
```

### Rewarded load fail

Ví dụ:

```text
ad_evt_lf_rq_rw1234_rt1_ec204_net1_f
```

Ý nghĩa:

- rewarded retry lần 1
- error `204`
- online
- speed fast

### Rewarded impression / reward

Ví dụ:

```text
ad_evt_imp_sh_rw1234_rewarded
ad_evt_rwd_sh_rw1234_rewarded
```

## 5. Rect tracking hiện tại

Rect callback được bắn ở:

- [Rect_GroupControllerBase.cs](/d:/Unity%20projects/BG-Library-Full-Module/Assets/BG_Lib/NetCore/Mediation%20Scripts/Controller/Rect_GroupControllerBase.cs)

### Banner request API

Ví dụ:

```text
ad_gr_bn_rq_bn720jy_ini_fb_api
```

### Banner first load success

Ví dụ:

```text
ad_evt_ls_rq_bn720jy_ini_fb_f
```

### Banner refresh success

Ví dụ:

```text
ad_evt_ls_rq_bn720jy_ini_fb_rl
```

Lưu ý:

- refresh success hiện dùng tail `ls_rl`
- context vẫn lấy từ lần request context gần nhất của controller

### Banner load fail

Ví dụ:

```text
ad_evt_lf_rq_bn720jy_ini_fb_ec3_net0
```

Lưu ý:

- rect load fail hiện không có speed token

### Banner impression

Ví dụ:

```text
ad_evt_imp_sh_bn720jy_fb
```

## 6. Rule đang đúng theo code hiện tại

- FS `OnAdLoadFailedEvent(...)` có bắn cả:
  - `NetTrackingSystem.LoadGroupFail(...)`
  - `NetEventSystem.OnFsLoadFailed`
- Rect `OnAdLoadFailedEvent(...)` có bắn:
  - `NetTrackingSystem.LoadGroupFailNoSpeed(...)`
  - `NetEventSystem.OnRectLoadFailed`
- Tracking luôn đi trước `NetEventSystem` trong controller layer.

## 7. Gợi ý debug nhanh

Nếu muốn check FS load fail:

- tìm `ad_evt_lf_rq_fa`
- tìm `ad_evt_lf_rq_rw`

Nếu muốn check rect load fail:

- tìm `ad_evt_lf_rq_bn`
- tìm `ad_evt_lf_rq_mr`

Nếu muốn check request API:

- tìm `ad_gr_fa_rq`
- tìm `ad_gr_rw_rq`
- tìm `ad_gr_bn_rq`
- tìm `ad_gr_mr_rq`
