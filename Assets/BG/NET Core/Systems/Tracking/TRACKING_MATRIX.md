# Tracking Matrix

## Rule

Phần này dùng để giải thích **cách đọc tên event** của `NetTrackingSystem`.

### 1. Hai họ event chính

Matrix này dùng 2 họ event:

- `command`: event bắt đầu flow hoặc đang đi trong flow
- `callback`: event trả về sau `request` hoặc sau `show`

#### Command

```text
ad_{layer}_{channel?}_{action}_{id?}_{context?}_{target?}_n_{reason?}_api
```

Trong đó:

- `layer`: `atsy / sy / ac / gr`
- `channel`: `al / ar / fa / rw / bn / mr / pu / cl`
- `action`: `rq / sh / act / hid`
- `id`: group id rút gọn như `ao1234`, `fa1234`, `bn1234`
- `context`: `ini / rtN / idl / hdr / sfr`
- `target`: `pos` hoặc `groupName`
- `_n_`: marker báo flow bị ngắt
- `reason`: lý do flow bị ngắt
- `api`: flow đã chạm API

Reason token hay gặp:

- `cfg / dis / nready / upos / size0 / ec123 ...`

Current positive-entry rule:

- `system` keeps the only positive entry for `request / show / act / hid`
- `group` keeps only the terminal `*_api`
- `adcore` and non-API positive `group` entries are removed
- all `_n_` drops stay unchanged
- all `ad_evt_*` callbacks stay unchanged

Event / tracking ordering rule:

- when a controller emits both tracking and `NetEventSystem`, tracking goes first and event goes after
- the controller layer owns this ordering; helper / mediation logic should avoid firing `NetEventSystem` directly when controller can do it
- fallback listeners may react to `NetEventSystem`, so this ordering keeps the current candidate's tracking closed before backup flow starts

Callback semantics rule:

- a real SDK callback must always surface as `ad_evt_*`
- if a local lifecycle event does not produce `ad_evt_*`, do not read it as a callback tracking event
- pre-API local fails such as `not ready / dequeue null / guard fail` stay in `command` flow, not `callback` flow

Ví dụ:
- `ad_atsy_al_sh_apLaunc`
- `ad_sy_rw_sh_x2coin_n_cfg`
- `ad_gr_sh_bn1234_fb_api`
- `ad_gr_fa_rq_fa1234_ini_gamePlay`
- `ad_gr_fa_rq_fa1234_ini_n_inited_gamePlay`
- `ad_gr_fa_rq_fa1234_ini_gamePlay_api`

#### Callback

```text
ad_evt_{evtName}_{action}_{id}_{context?}_{target?}_{reason?}_{net?}_{speed?}
```

Trong đó:

- `evtName`: `dsp / clk / imp / cls / rwd / shf / ls / lf`
- `action`: callback chỉ đi với `sh` hoặc `rq`
- khi đã có `evt` thì **không có `layer`**

Ví dụ:

- `ad_evt_dsp_sh_bn1234_fb`
- `ad_evt_shf_sh_fa1234_homegate_ec123_net1`
- `ad_evt_ls_rq_ao1234_ini_f`
- `ad_evt_lf_rq_fa1234_ini_gamePlay_ec123_net1_f`

### 2. Context của request

Flow `group request` dùng các token ngắn để mô tả bối cảnh request gần nhất:

- `ini`: init
- `rtN`: retry lần `N`
- `idl`: idle reload
- `hdr`: hide reload
- `sfr`: show fail reload

Rule cần nhớ:

- nếu command có `context` thì `target` luôn đứng sau `context`
- với `FA` và `PU`, `groupName` được coi là `target`

Ví dụ:

- `ad_gr_fa_rq_fa1234_ini_gamePlay`
- `ad_gr_pu_rq_pu1234_ini_naFull`

### 3. Callback sau request và sau show

Callback load success / fail:

- `ad_evt_ls_rq_{id}_{context}_{speed}`
- với rect, các lần `load success` sau success đầu tiên dùng `ad_evt_ls_rq_{id}_{context}_rl`
- `ad_evt_lf_rq_{id}_{context}_{target?}_ec{code}_net{0|1}_{speed}`
- với rect, `load fail` không dùng speed: `ad_evt_lf_rq_{id}_{context}_{target?}_ec{code}_net{0|1}`

Trong đó:

- `f`: `< 5000ms`
- `n`: `5000ms - 10000ms`
- `s`: `10001ms - 15000ms`
- `x`: `> 15000ms`
- `rl`: rect refresh success, không dùng speed bucket

Callback runtime sau show:

- `ad_evt_dsp_sh_{id}_{target}`
- `ad_evt_clk_sh_{id}_{target}`
- `ad_evt_imp_sh_{id}_{target}`
- `ad_evt_cls_sh_{id}_{target}`
- `ad_evt_rwd_sh_{id}_{target}`
- `ad_evt_shf_sh_{id}_{target}_ec{code}_net{0|1}`

### 4. Rule rút gọn cho `pos` và `groupName`

`pos` và `groupName` không giữ nguyên `_` khi lên tracking.

Hiện tại rule đọc nên hiểu là:

- tách từ theo `_`
- tách tiếp nếu có chuyển giữa chữ và số
- tách tiếp nếu đang ở camelCase
- sau đó ghép lại theo kiểu camelCase ngắn gọn

Ví dụ:

- `a_lch -> aLch`
- `a_res -> aRes`
- `fa_all -> faAll`
- `na_full -> naFull`

Nếu token đã là một từ liền hoặc đã có chữ hoa hợp lệ thì sẽ giữ tinh thần cũ, chỉ bị rút gọn nếu quá dài.

### 5. Rule đọc `groupId`

Trong matrix này, `groupId` là token đại diện cho group thực đang được dùng để tracking, ví dụ:

- `ao1234`
- `rw1234`
- `fa1234`

Khi đọc bảng bên dưới, hãy hiểu đây là **id rút gọn để filter event**, không phải full ad unit id thật.

### 6. Token action và layer

- `rq`: request
- `sh`: show
- `act`: activate
- `hid`: hide
- `atsy`: auto-show system
- `sy`: system
- `ac`: adcore
- `gr`: group
- `n`: fail / negative branch
- `api`: đã chạm SDK API hoặc UI API

`hide` là command flow riêng và dùng token `hid`.

<table>
  <thead>
    <tr>
      <th></th>
      <th>A</th>
      <th>B</th>
      <th>C</th>
      <th>D</th>
      <th>E</th>
      <th>F</th>
      <th>G</th>
      <th>H</th>
      <th>I</th>
      <th>J</th>
      <th>K</th>
    </tr>
    <tr>
      <th>1</th>
      <th>STT</th>
      <th>Layer</th>
      <th>Flow</th>
      <th>AppLaunch</th>
      <th>AppResume</th>
      <th>Rewarded</th>
      <th>ForceAd</th>
      <th>Popup</th>
      <th>Banner</th>
      <th>Mrec</th>
      <th>Collap</th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <th>2</th>
      <td rowspan="2">1</td>
      <td rowspan="2">System</td>
      <td>AutoShow</td>
      <td>
        <strong>ad_atsy_al_sh_apLaunc</strong><br>
        <br>
        Drop =&gt;<br>
        ad_atsy_al_sh_apLaunc_n_tmo<br>
        ad_atsy_al_sh_apLaunc_n_iap<br>
        ad_atsy_al_sh_apLaunc_n_cfg<br>
        ad_atsy_al_sh_apLaunc_n_ign<br>
      </td>
      <td>
        <strong>ad_atsy_ar_sh_apResum</strong><br>
        <br>
        Drop =&gt;<br>
        ad_atsy_ar_sh_apResum_n_iap<br>
        ad_atsy_ar_sh_apResum_n_cfg<br>
        ad_atsy_ar_sh_apResum_n_ign<br>
        ad_atsy_ar_sh_apResum_n_fsblk<br>
        ad_atsy_ar_sh_apResum_n_rcfg<br>
        ad_atsy_ar_sh_apResum_n_nready<br>
      </td>
      <td>&nbsp;</td>
      <td>
        <strong>ad_atsy_fa_sh_homegate</strong><br>
        <br>
        Drop =&gt;<br>
        ad_atsy_fa_sh_homegate_n_iap<br>
        ad_atsy_fa_sh_homegate_n_null<br>
        ad_atsy_fa_sh_homegate_n_block<br>
        ad_atsy_fa_sh_homegate_n_ign<br>
        ad_atsy_fa_sh_homegate_n_nready<br>
      </td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
    </tr>
    <tr>
      <th>3</th>
      <td>Show</td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
      <td>
        <strong>ad_sy_rw_sh_x2coin</strong><br>
        <br>
        Drop =&gt;<br>
        ad_sy_rw_sh_x2coin_n_cfg<br>
        ad_sy_rw_sh_x2coin_n_ign<br>
      </td>
      <td>
        <strong>ad_sy_fa_sh_homegate</strong><br>
        <br>
        Drop =&gt;<br>
        ad_sy_fa_sh_homegate_n_iap<br>
        ad_sy_fa_sh_homegate_n_cfg<br>
        ad_sy_fa_sh_homegate_n_pos<br>
        ad_sy_fa_sh_homegate_n_cap<br>
        ad_sy_fa_sh_homegate_n_ign<br>
      </td>
      <td>
        <strong>ad_sy_pu_sh_offerwall</strong><br>
        <br>
        Drop =&gt;<br>
        ad_sy_pu_sh_offerwall_n_iap<br>
        ad_sy_pu_sh_offerwall_n_cfg<br>
        ad_sy_pu_sh_offerwall_n_pos<br>
      </td>
      <td>
        <strong>ad_sy_bn_sh_fb</strong><br>
        <br>
        Drop =&gt;<br>
        ad_sy_bn_sh_fb_n_iap<br>
        ad_sy_bn_sh_fb_n_cfg<br>
      </td>
      <td>
        <strong>ad_sy_mr_sh_mrDef</strong><br>
        <br>
        Drop =&gt;<br>
        ad_sy_mr_sh_mrDef_n_iap<br>
        ad_sy_mr_sh_mrDef_n_cfg<br>
      </td>
      <td>
        <strong>ad_sy_cl_sh_clDef</strong><br>
        <br>
        Drop =&gt;<br>
        ad_sy_cl_sh_clDef_n_iap<br>
        ad_sy_cl_sh_clDef_n_cfg<br>
      </td>
    </tr>
    <tr>
      <th>4</th>
      <td>2</td>
      <td>AdCore</td>
      <td>Show</td>
      <td>
        <br>
        Drop =&gt;<br>
        ad_ac_al_sh_apLaunc_n_group<br>
      </td>
      <td>
        <br>
        Drop =&gt;<br>
        ad_ac_ar_sh_apResum_n_group<br>
      </td>
      <td>
        <br>
        Drop =&gt;<br>
        ad_ac_rw_sh_x2coin_n_group<br>
      </td>
      <td>
        <br>
        Drop =&gt;<br>
        ad_ac_fa_sh_homegate_n_group<br>
      </td>
      <td>
        <br>
        Drop =&gt;<br>
        ad_ac_pu_sh_offerwall_n_group<br>
      </td>
      <td>
        <br>
        Drop =&gt;<br>
        ad_ac_bn_sh_fb_n_group<br>
      </td>
      <td>
        <br>
        Drop =&gt;<br>
        ad_ac_mr_sh_mrDef_n_group<br>
      </td>
      <td>
        <br>
        Drop =&gt;<br>
        ad_ac_cl_sh_clDef_n_group<br>
      </td>
    </tr>
    <tr>
      <th>5</th>
      <td>3</td>
      <td>Group</td>
      <td>Show</td>
      <td>
        <br>
        Drop =&gt;<br>
        ad_gr_sh_ao1234_apLaunc_n_nready<br>
        <br>
        API =&gt;<br>
        <strong>ad_gr_sh_ao1234_apLaunc_api</strong><br>
      </td>
      <td>
        <br>
        Drop =&gt;<br>
        ad_gr_sh_ao1234_apResum_n_nready<br>
        <br>
        API =&gt;<br>
        <strong>ad_gr_sh_ao1234_apResum_api</strong><br>
      </td>
      <td>
        <br>
        Drop =&gt;<br>
        ad_gr_sh_rw1234_x2coin_n_nready<br>
        <br>
        API =&gt;<br>
        <strong>ad_gr_sh_rw1234_x2coin_api</strong><br>
      </td>
      <td>
        <br>
        Drop =&gt;<br>
        ad_gr_sh_fa1234_homegate_n_nready<br>
        <br>
        API =&gt;<br>
        <strong>ad_gr_sh_fa1234_homegate_api</strong><br>
      </td>
      <td>
        <br>
        Drop =&gt;<br>
        ad_gr_sh_pu1234_offerwall_n_nready<br>
        ad_gr_sh_pu1234_offerwall_n_showing<br>
        ad_gr_sh_pu1234_offerwall_n_upos<br>
        ad_gr_sh_pu1234_offerwall_n_size0<br>
        <br>
        API =&gt;<br>
        <strong>ad_gr_sh_pu1234_offerwall_api</strong><br>
      </td>
      <td>
        <br>
        Drop =&gt;<br>
        ad_gr_sh_bn1234_fb_n_nready<br>
        ad_gr_sh_bn1234_fb_n_showing<br>
        <br>
        API =&gt;<br>
        <strong>ad_gr_sh_bn1234_fb_api</strong><br>
      </td>
      <td>
        <br>
        Drop =&gt;<br>
        ad_gr_sh_mr1234_mrDef_n_nready<br>
        ad_gr_sh_mr1234_mrDef_n_showing<br>
        <br>
        API =&gt;<br>
        <strong>ad_gr_sh_mr1234_mrDef_api</strong><br>
      </td>
      <td>
        <br>
        Drop =&gt;<br>
        ad_gr_sh_cl1234_clDef_n_nready<br>
        ad_gr_sh_cl1234_clDef_n_showing<br>
        <br>
        API =&gt;<br>
        <strong>ad_gr_sh_cl1234_clDef_api</strong><br>
      </td>
    </tr>
    <tr>
      <th>6</th>
      <td>&nbsp;</td>
      <td>Legend</td>
      <td>Giải nghĩa</td>
      <td>
        <code>apLaunc</code> = token runtime rút gọn từ <code>app_launch</code> của AppLaunch.<br>
        Group id có thể là <code>ao1234</code> nếu comeback dùng AppOpen, hoặc <code>fa1234</code> nếu comeback dùng ForceAd.<br>
        Config Android hiện tại: <code>fa1234</code> vì <code>launchAdType = FA</code> với group <code>laun</code>.<br>
        <code>1234</code> chỉ là ví dụ cho 4 ký tự stable id.
      </td>
      <td>
        <code>apResum</code> = token runtime rút gọn từ <code>app_resume</code> của AppResume.<br>
        Group id có thể là <code>ao1234</code> nếu comeback dùng AppOpen, hoặc <code>fa1234</code> nếu comeback dùng ForceAd.<br>
        Config Android hiện tại: <code>ao1234</code> vì <code>resumeAdType = AO</code>.<br>
        <code>1234</code> chỉ là ví dụ cho 4 ký tự stable id.
      </td>
      <td>
        <code>x2coin</code> = ví dụ pos truyền vào từ caller, có thể thay đổi theo bối cảnh.<br>
        Group id chuẩn của channel này là <code>rw1234</code> vì Rewarded đi vào rewarded group.<br>
        Config Android hiện tại: <code>rw1234</code> với <code>rewardedUnit</code> đang dùng mediation Admob.<br>
        <code>1234</code> chỉ là ví dụ cho 4 ký tự stable id.
      </td>
      <td>
        <code>homegate</code> = ví dụ pos truyền vào từ caller, có thể thay đổi theo bối cảnh.<br>
        Group id chuẩn của channel này là <code>fa1234</code> vì ForceAd đi vào force ad group.<br>
        Một pos sẽ được map sang <code>groupName</code> tương ứng trong <code>forceAdGroups</code> trước khi xuống group.<br>
        <code>1234</code> chỉ là ví dụ cho 4 ký tự stable id.
      </td>
      <td>
        <code>offerwall</code> = ví dụ pos truyền vào từ caller, có thể thay đổi theo bối cảnh.<br>
        Group id chuẩn của channel này là <code>pu1234</code> vì Popup đi vào popup group.<br>
        Một pos sẽ được map sang <code>groupName</code> tương ứng trong <code>popupGroups</code> trước khi xuống group.<br>
        <code>1234</code> chỉ là ví dụ cho 4 ký tự stable id.
      </td>
      <td>
        <code>fb/ft/tl/tr/bl/br</code> = placement token runtime của Banner, map lần lượt tới <code>FullBottom/FullTop/TopLeft/TopRight/BottomLeft/BottomRight</code>.<br>
        Group id chuẩn của channel này là <code>bn1234</code> vì Banner đi vào banner group.<br>
        Config Android hiện tại: <code>bn1234</code> với <code>bannerUnit.mediationPriority = Admob</code>, nhưng prefix tracking vẫn giữ là <code>bn</code>.<br>
        <code>1234</code> chỉ là ví dụ cho 4 ký tự stable id.
      </td>
      <td>
        <code>mrDef</code> = pos mặc định riêng của Mrec vì API <code>Show()</code> không nhận pos từ caller.<br>
        Group id chuẩn của channel này là <code>mr1234</code> vì Mrec đi vào mrec group.<br>
        Config Android hiện tại: <code>mr1234</code> và luồng resolve group đang là Admob only trong <code>AdCore_MainAndroid</code>.<br>
        <code>1234</code> chỉ là ví dụ cho 4 ký tự stable id.
      </td>
      <td>
        <code>clDef</code> = pos mặc định riêng của Collap vì API <code>Show()</code> không nhận pos từ caller.<br>
        Group id chuẩn của channel này là <code>cl1234</code> vì Collap đi vào collap group.<br>
        Config Android hiện tại: <code>cl1234</code> và luồng resolve group đang là Android only trong <code>AdCore_MainAndroid</code>.<br>
        <code>1234</code> chỉ là ví dụ cho 4 ký tự stable id.
      </td>
    </tr>
    <tr>
      <th>7</th>
      <td colspan="11">&nbsp;</td>
    </tr>
    <tr>
      <th>8</th>
      <td>4</td>
      <td>System</td>
      <td>Request</td>
      <td>
        <strong>ad_sy_al_rq</strong><br>
        <br>
        Drop =&gt;<br>
        ad_sy_al_rq_n_iap<br>
        ad_sy_al_rq_n_cfg<br>
      </td>
      <td>
        <strong>ad_sy_ar_rq</strong><br>
        <br>
        Drop =&gt;<br>
        ad_sy_ar_rq_n_iap<br>
        ad_sy_ar_rq_n_cfg<br>
      </td>
      <td>
        <strong>ad_sy_rw_rq</strong><br>
        <br>
        Drop =&gt;<br>
        ad_sy_rw_rq_n_cfg<br>
      </td>
      <td>
        <strong>ad_sy_fa_rq_gamePlay</strong><br>
        <br>
        Drop =&gt;<br>
        ad_sy_fa_rq_gamePlay_n_iap<br>
        ad_sy_fa_rq_gamePlay_n_cfg<br>
      </td>
      <td>
        <strong>ad_sy_pu_rq_naFull</strong><br>
        <br>
        Drop =&gt;<br>
        ad_sy_pu_rq_naFull_n_iap<br>
        ad_sy_pu_rq_naFull_n_cfg<br>
      </td>
      <td>
        <strong>ad_sy_bn_rq_fb</strong><br>
        <br>
        Drop =&gt;<br>
        ad_sy_bn_rq_fb_n_iap<br>
        ad_sy_bn_rq_fb_n_cfg<br>
      </td>
      <td>
        <strong>ad_sy_mr_rq</strong><br>
        <br>
        Drop =&gt;<br>
        ad_sy_mr_rq_n_iap<br>
        ad_sy_mr_rq_n_cfg<br>
      </td>
      <td>
        <strong>ad_sy_cl_rq</strong><br>
        <br>
        Drop =&gt;<br>
        ad_sy_cl_rq_n_iap<br>
        ad_sy_cl_rq_n_cfg<br>
      </td>
      <td>&nbsp;</td>
    </tr>
    <tr>
      <th>9</th>
      <td>5</td>
      <td>AdCore</td>
      <td>Request</td>
      <td>
        <br>
        Drop =&gt;<br>
        ad_ac_al_rq_n_group<br>
      </td>
      <td>
        <br>
        Drop =&gt;<br>
        ad_ac_ar_rq_n_group<br>
      </td>
      <td>
        <br>
        Drop =&gt;<br>
        ad_ac_rw_rq_n_group<br>
      </td>
      <td>
        <br>
        Drop =&gt;<br>
        ad_ac_fa_rq_gamePlay_n_group<br>
      </td>
      <td>
        <br>
        Drop =&gt;<br>
        ad_ac_pu_rq_naFull_n_group<br>
      </td>
      <td>
        <br>
        Drop =&gt;<br>
        ad_ac_bn_rq_fb_n_group<br>
      </td>
      <td>
        <br>
        Drop =&gt;<br>
        ad_ac_mr_rq_n_group<br>
      </td>
      <td>
        <br>
        Drop =&gt;<br>
        ad_ac_cl_rq_n_group<br>
      </td>
      <td>&nbsp;</td>
    </tr>
    <tr>
      <th>10</th>
      <td>6</td>
      <td>Group</td>
      <td>Init</td>
      <td>
        <br>
        Drop =&gt;<br>
        ad_gr_al_rq_ao1234_ini_n_inited<br>
        ad_gr_al_rq_ao1234_ini_n_noid<br>
        <br>
        API =&gt;<br>
        <strong>ad_gr_al_rq_ao1234_ini_api</strong><br>
      </td>
      <td>
        <br>
        Drop =&gt;<br>
        ad_gr_ar_rq_ao1234_ini_n_inited<br>
        ad_gr_ar_rq_ao1234_ini_n_noid<br>
        <br>
        API =&gt;<br>
        <strong>ad_gr_ar_rq_ao1234_ini_api</strong><br>
      </td>
      <td>
        <br>
        Drop =&gt;<br>
        ad_gr_rw_rq_rw1234_ini_n_inited<br>
        ad_gr_rw_rq_rw1234_ini_n_noid<br>
        <br>
        API =&gt;<br>
        <strong>ad_gr_rw_rq_rw1234_ini_api</strong><br>
      </td>
      <td>
        <br>
        Drop =&gt;<br>
        ad_gr_fa_rq_fa1234_ini_n_inited_gamePlay<br>
        ad_gr_fa_rq_fa1234_ini_n_noid_gamePlay<br>
        <br>
        API =&gt;<br>
        <strong>ad_gr_fa_rq_fa1234_ini_gamePlay_api</strong><br>
      </td>
      <td>
        <br>
        Drop =&gt;<br>
        ad_gr_pu_rq_pu1234_ini_n_inited_naFull<br>
        ad_gr_pu_rq_pu1234_ini_n_noid_naFull<br>
        ad_gr_pu_rq_pu1234_ini_n_aload_naFull<br>
        <br>
        API =&gt;<br>
        <strong>ad_gr_pu_rq_pu1234_ini_naFull_api</strong><br>
      </td>
      <td>
        <br>
        Drop =&gt;<br>
        ad_gr_bn_rq_bn1234_ini_n_inited_fb<br>
        ad_gr_bn_rq_bn1234_ini_n_noid_fb<br>
        ad_gr_bn_rq_bn1234_ini_n_aload_fb<br>
        <br>
        API =&gt;<br>
        <strong>ad_gr_bn_rq_bn1234_ini_fb_api</strong><br>
      </td>
      <td>
        <br>
        Drop =&gt;<br>
        ad_gr_mr_rq_mr1234_ini_n_inited<br>
        ad_gr_mr_rq_mr1234_ini_n_noid<br>
        ad_gr_mr_rq_mr1234_ini_n_aload<br>
        <br>
        API =&gt;<br>
        <strong>ad_gr_mr_rq_mr1234_ini_api</strong><br>
      </td>
      <td>
        <br>
        Drop =&gt;<br>
        ad_gr_cl_rq_cl1234_ini_n_inited<br>
        ad_gr_cl_rq_cl1234_ini_n_noid<br>
        ad_gr_cl_rq_cl1234_ini_n_aload<br>
        <br>
        API =&gt;<br>
        <strong>ad_gr_cl_rq_cl1234_ini_api</strong><br>
      </td>
      <td>&nbsp;</td>
    </tr>
    <tr>
      <th>11</th>
      <td>7</td>
      <td>Group</td>
      <td>Retry</td>
      <td>
        <br>
        Drop =&gt;<br>
        ad_gr_al_rq_ao1234_rt1_n_gate<br>
        ad_gr_al_rq_ao1234_rt1_n_host<br>
        ad_gr_al_rq_ao1234_rt1_n_dis<br>
        ad_gr_al_rq_ao1234_rt1_n_aload<br>
        ad_gr_al_rq_ao1234_rt1_n_loading<br>
        <br>
        API =&gt;<br>
        <strong>ad_gr_al_rq_ao1234_rt1_api</strong><br>
      </td>
      <td>
        <br>
        Drop =&gt;<br>
        ad_gr_ar_rq_ao1234_rt1_n_gate<br>
        ad_gr_ar_rq_ao1234_rt1_n_host<br>
        ad_gr_ar_rq_ao1234_rt1_n_dis<br>
        ad_gr_ar_rq_ao1234_rt1_n_aload<br>
        ad_gr_ar_rq_ao1234_rt1_n_loading<br>
        <br>
        API =&gt;<br>
        <strong>ad_gr_ar_rq_ao1234_rt1_api</strong><br>
      </td>
      <td>
        <br>
        Drop =&gt;<br>
        ad_gr_rw_rq_rw1234_rt1_n_gate<br>
        ad_gr_rw_rq_rw1234_rt1_n_host<br>
        ad_gr_rw_rq_rw1234_rt1_n_dis<br>
        ad_gr_rw_rq_rw1234_rt1_n_aload<br>
        ad_gr_rw_rq_rw1234_rt1_n_loading<br>
        <br>
        API =&gt;<br>
        <strong>ad_gr_rw_rq_rw1234_rt1_api</strong><br>
      </td>
      <td>
        <br>
        Drop =&gt;<br>
        ad_gr_fa_rq_fa1234_rt1_n_gate_gamePlay<br>
        ad_gr_fa_rq_fa1234_rt1_n_host_gamePlay<br>
        ad_gr_fa_rq_fa1234_rt1_n_dis_gamePlay<br>
        ad_gr_fa_rq_fa1234_rt1_n_aload_gamePlay<br>
        ad_gr_fa_rq_fa1234_rt1_n_loading_gamePlay<br>
        <br>
        API =&gt;<br>
        <strong>ad_gr_fa_rq_fa1234_rt1_gamePlay_api</strong><br>
      </td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
    </tr>
    <tr>
      <th>12</th>
      <td>8</td>
      <td>Group</td>
      <td>Idle</td>
      <td>
        <br>
        Drop =&gt;<br>
        ad_gr_al_rq_ao1234_idl_n_retry<br>
        ad_gr_al_rq_ao1234_idl_n_dis<br>
        ad_gr_al_rq_ao1234_idl_n_aload<br>
        ad_gr_al_rq_ao1234_idl_n_loading<br>
        <br>
        API =&gt;<br>
        <strong>ad_gr_al_rq_ao1234_idl_api</strong><br>
      </td>
      <td>
        <br>
        Drop =&gt;<br>
        ad_gr_ar_rq_ao1234_idl_n_retry<br>
        ad_gr_ar_rq_ao1234_idl_n_dis<br>
        ad_gr_ar_rq_ao1234_idl_n_aload<br>
        ad_gr_ar_rq_ao1234_idl_n_loading<br>
        <br>
        API =&gt;<br>
        <strong>ad_gr_ar_rq_ao1234_idl_api</strong><br>
      </td>
      <td>
        <br>
        Drop =&gt;<br>
        ad_gr_rw_rq_rw1234_idl_n_retry<br>
        ad_gr_rw_rq_rw1234_idl_n_dis<br>
        ad_gr_rw_rq_rw1234_idl_n_aload<br>
        ad_gr_rw_rq_rw1234_idl_n_loading<br>
        <br>
        API =&gt;<br>
        <strong>ad_gr_rw_rq_rw1234_idl_api</strong><br>
      </td>
      <td>
        <br>
        Drop =&gt;<br>
        ad_gr_fa_rq_fa1234_idl_n_retry_gamePlay<br>
        ad_gr_fa_rq_fa1234_idl_n_dis_gamePlay<br>
        ad_gr_fa_rq_fa1234_idl_n_aload_gamePlay<br>
        ad_gr_fa_rq_fa1234_idl_n_loading_gamePlay<br>
        <br>
        API =&gt;<br>
        <strong>ad_gr_fa_rq_fa1234_idl_gamePlay_api</strong><br>
      </td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
    </tr>
    <tr>
      <th>13</th>
      <td>9</td>
      <td>Group</td>
      <td>HideReload</td>
      <td>
        <br>
        Drop =&gt;<br>
        ad_gr_al_rq_ao1234_hdr_n_budget<br>
        ad_gr_al_rq_ao1234_hdr_n_dis<br>
        ad_gr_al_rq_ao1234_hdr_n_aload<br>
        ad_gr_al_rq_ao1234_hdr_n_loading<br>
        <br>
        API =&gt;<br>
        <strong>ad_gr_al_rq_ao1234_hdr_api</strong><br>
      </td>
      <td>
        <br>
        Drop =&gt;<br>
        ad_gr_ar_rq_ao1234_hdr_n_budget<br>
        ad_gr_ar_rq_ao1234_hdr_n_dis<br>
        ad_gr_ar_rq_ao1234_hdr_n_aload<br>
        ad_gr_ar_rq_ao1234_hdr_n_loading<br>
        <br>
        API =&gt;<br>
        <strong>ad_gr_ar_rq_ao1234_hdr_api</strong><br>
      </td>
      <td>
        <br>
        Drop =&gt;<br>
        ad_gr_rw_rq_rw1234_hdr_n_budget<br>
        ad_gr_rw_rq_rw1234_hdr_n_dis<br>
        ad_gr_rw_rq_rw1234_hdr_n_aload<br>
        ad_gr_rw_rq_rw1234_hdr_n_loading<br>
        <br>
        API =&gt;<br>
        <strong>ad_gr_rw_rq_rw1234_hdr_api</strong><br>
      </td>
      <td>
        <br>
        Drop =&gt;<br>
        ad_gr_fa_rq_fa1234_hdr_n_budget_gamePlay<br>
        ad_gr_fa_rq_fa1234_hdr_n_dis_gamePlay<br>
        ad_gr_fa_rq_fa1234_hdr_n_aload_gamePlay<br>
        ad_gr_fa_rq_fa1234_hdr_n_loading_gamePlay<br>
        <br>
        API =&gt;<br>
        <strong>ad_gr_fa_rq_fa1234_hdr_gamePlay_api</strong><br>
      </td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
    </tr>
    <tr>
      <th>14</th>
      <td>10</td>
      <td>Group</td>
      <td>DisplayFailReload</td>
      <td>
        <br>
        Drop =&gt;<br>
        ad_gr_al_rq_ao1234_sfr_n_dis<br>
        ad_gr_al_rq_ao1234_sfr_n_aload<br>
        ad_gr_al_rq_ao1234_sfr_n_loading<br>
        <br>
        API =&gt;<br>
        <strong>ad_gr_al_rq_ao1234_sfr_api</strong><br>
      </td>
      <td>
        <br>
        Drop =&gt;<br>
        ad_gr_ar_rq_ao1234_sfr_n_dis<br>
        ad_gr_ar_rq_ao1234_sfr_n_aload<br>
        ad_gr_ar_rq_ao1234_sfr_n_loading<br>
        <br>
        API =&gt;<br>
        <strong>ad_gr_ar_rq_ao1234_sfr_api</strong><br>
      </td>
      <td>
        <br>
        Drop =&gt;<br>
        ad_gr_rw_rq_rw1234_sfr_n_dis<br>
        ad_gr_rw_rq_rw1234_sfr_n_aload<br>
        ad_gr_rw_rq_rw1234_sfr_n_loading<br>
        <br>
        API =&gt;<br>
        <strong>ad_gr_rw_rq_rw1234_sfr_api</strong><br>
      </td>
      <td>
        <br>
        Drop =&gt;<br>
        ad_gr_fa_rq_fa1234_sfr_n_dis_gamePlay<br>
        ad_gr_fa_rq_fa1234_sfr_n_aload_gamePlay<br>
        ad_gr_fa_rq_fa1234_sfr_n_loading_gamePlay<br>
        <br>
        API =&gt;<br>
        <strong>ad_gr_fa_rq_fa1234_sfr_gamePlay_api</strong><br>
      </td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
    </tr>
    <tr>
      <th>15</th>
      <td>&nbsp;</td>
      <td>Legend</td>
      <td>Giải nghĩa</td>
      <td>
        Flow request của channel này không mang pos.<br>
        Tầng <code>system</code> và <code>adcore</code> lần lượt có dạng <code>ad_sy_al_rq</code> và <code>ad_ac_al_rq</code>.<br>
        Context của <code>group request</code> dùng bộ viết tắt <code>ini / rtN / idl / hdr / sfr</code>.<br>
        Tầng <code>group</code> dùng ví dụ <code>ao1234</code> là group id.<br>
        Có thể filter API của group theo mẫu <code>ad_gr_al_rq_ao1234_{context}_api</code>.
      </td>
      <td>
        Flow request của channel này không mang pos.<br>
        Tầng <code>system</code> và <code>adcore</code> lần lượt có dạng <code>ad_sy_ar_rq</code> và <code>ad_ac_ar_rq</code>.<br>
        Context của <code>group request</code> dùng bộ viết tắt <code>ini / rtN / idl / hdr / sfr</code>.<br>
        Tầng <code>group</code> dùng ví dụ <code>ao1234</code> là group id.<br>
        Có thể filter API của group theo mẫu <code>ad_gr_ar_rq_ao1234_{context}_api</code>.
      </td>
      <td>
        Flow request của channel này không mang pos.<br>
        Tầng <code>system</code> và <code>adcore</code> lần lượt có dạng <code>ad_sy_rw_rq</code> và <code>ad_ac_rw_rq</code>.<br>
        Context của <code>group request</code> dùng bộ viết tắt <code>ini / rtN / idl / hdr / sfr</code>.<br>
        Tầng <code>group</code> dùng ví dụ <code>rw1234</code> là group id.<br>
        Có thể filter API của group theo mẫu <code>ad_gr_rw_rq_rw1234_{context}_api</code>.
      </td>
      <td>
        Flow request của channel này không mang pos.<br>
        Tầng <code>system</code> và <code>adcore</code> dùng ví dụ <code>gamePlay</code> là <code>groupName</code>.<br>
        Context của <code>group request</code> dùng bộ viết tắt <code>ini / rtN / idl / hdr / sfr</code>.<br>
        Từ tầng <code>group</code> trở xuống dùng cụm bắt buộc <code>ad_gr_fa_rq_fa1234_{context}_gamePlay</code> để filter.<br>
        Có thể filter API bằng mẫu <code>ad_gr_fa_rq_fa1234_{context}_gamePlay_api</code>.
      </td>
      <td>
        Flow request của channel này không mang pos.<br>
        Tầng <code>system</code> và <code>adcore</code> dùng ví dụ <code>naFull</code> là <code>groupName</code>.<br>
        Context của <code>group request</code> hiện chỉ dùng <code>ini</code>.<br>
        Tầng <code>group</code> dùng cụm <code>ad_gr_pu_rq_pu1234_ini_naFull</code>.<br>
        Có thể filter API bằng mẫu <code>ad_gr_pu_rq_pu1234_ini_naFull_api</code>.<br>
        Hiện request của <code>Popup</code> chỉ có context <code>ini</code>.
      </td>
      <td>
        Flow request của channel này mang placement token như <code>fb/ft/tl/tr/bl/br</code>.<br>
        Tầng <code>system</code> và <code>adcore</code> lần lượt có dạng <code>ad_sy_bn_rq_fb</code> và <code>ad_ac_bn_rq_fb</code>.<br>
        Context của <code>group request</code> hiện chỉ dùng <code>ini</code>.<br>
        Vì luồng init không đi qua <code>groupName</code>, tầng <code>group</code> dùng ví dụ <code>bn1234</code> là group id và <code>fb</code> là placement.<br>
        Có thể filter API bằng mẫu <code>ad_gr_bn_rq_bn1234_ini_fb_api</code>.
      </td>
      <td>
        Flow request của channel này không mang pos.<br>
        Tầng <code>system</code> và <code>adcore</code> lần lượt có dạng <code>ad_sy_mr_rq</code> và <code>ad_ac_mr_rq</code>.<br>
        Context của <code>group request</code> hiện chỉ dùng <code>ini</code>.<br>
        Vì luồng init không đi qua <code>groupName</code>, tầng <code>group</code> chỉ dùng ví dụ <code>mr1234</code> là group id.<br>
        Có thể filter API bằng mẫu <code>ad_gr_mr_rq_mr1234_ini_api</code>.
      </td>
      <td>
        Flow request của channel này không mang pos.<br>
        Tầng <code>system</code> và <code>adcore</code> lần lượt có dạng <code>ad_sy_cl_rq</code> và <code>ad_ac_cl_rq</code>.<br>
        Context của <code>group request</code> hiện chỉ dùng <code>ini</code>.<br>
        Vì luồng init không đi qua <code>groupName</code>, tầng <code>group</code> chỉ dùng ví dụ <code>cl1234</code> là group id.<br>
        Có thể filter API bằng mẫu <code>ad_gr_cl_rq_cl1234_ini_api</code>.
      </td>
    </tr>
    <tr>
      <th>16</th>
      <td colspan="11">&nbsp;</td>
    </tr>
    <tr>
      <th>17</th>
      <td>11</td>
      <td>Group</td>
      <td>LoadSuccess</td>
      <td>
        ad_evt_ls_rq_ao1234_ini_f<br>
        ad_evt_ls_rq_ao1234_rt1_f<br>
        ad_evt_ls_rq_ao1234_idl_f<br>
        ad_evt_ls_rq_ao1234_hdr_f<br>
        ad_evt_ls_rq_ao1234_sfr_f<br>
      </td>
      <td>
        ad_evt_ls_rq_ao1234_ini_f<br>
        ad_evt_ls_rq_ao1234_rt1_f<br>
        ad_evt_ls_rq_ao1234_idl_f<br>
        ad_evt_ls_rq_ao1234_hdr_f<br>
        ad_evt_ls_rq_ao1234_sfr_f<br>
      </td>
      <td>
        ad_evt_ls_rq_rw1234_ini_f<br>
        ad_evt_ls_rq_rw1234_rt1_f<br>
        ad_evt_ls_rq_rw1234_idl_f<br>
        ad_evt_ls_rq_rw1234_hdr_f<br>
        ad_evt_ls_rq_rw1234_sfr_f<br>
      </td>
      <td>
        ad_evt_ls_rq_fa1234_ini_gamePlay_f<br>
        ad_evt_ls_rq_fa1234_rt1_gamePlay_f<br>
        ad_evt_ls_rq_fa1234_idl_gamePlay_f<br>
        ad_evt_ls_rq_fa1234_hdr_gamePlay_f<br>
        ad_evt_ls_rq_fa1234_sfr_gamePlay_f<br>
      </td>
      <td>
        ad_evt_ls_rq_pu1234_ini_naFull_f<br>
        ad_evt_ls_rq_pu1234_idl_naFull_rl<br>
        ad_evt_ls_rq_pu1234_hdr_naFull_rl<br>
        ad_evt_ls_rq_pu1234_sfr_naFull_rl<br>
      </td>
      <td>
        ad_evt_ls_rq_bn1234_ini_fb_f<br>
        ad_evt_ls_rq_bn1234_idl_fb_rl<br>
        ad_evt_ls_rq_bn1234_hdr_fb_rl<br>
      </td>
      <td>
        ad_evt_ls_rq_mr1234_ini_f<br>
        ad_evt_ls_rq_mr1234_idl_rl<br>
        ad_evt_ls_rq_mr1234_hdr_rl<br>
      </td>
      <td>
        ad_evt_ls_rq_cl1234_ini_f<br>
        ad_evt_ls_rq_cl1234_idl_rl<br>
        ad_evt_ls_rq_cl1234_hdr_rl<br>
      </td>
    </tr>
    <tr>
      <th>18</th>
      <td>12</td>
      <td>Group</td>
      <td>LoadFail</td>
      <td>
        ad_evt_lf_rq_ao1234_ini_ec123_net1_f<br>
        ad_evt_lf_rq_ao1234_rt1_ec123_net1_f<br>
        ad_evt_lf_rq_ao1234_idl_ec123_net1_f<br>
        ad_evt_lf_rq_ao1234_hdr_ec123_net1_f<br>
        ad_evt_lf_rq_ao1234_sfr_ec123_net1_f<br>
      </td>
      <td>
        ad_evt_lf_rq_ao1234_ini_ec123_net1_f<br>
        ad_evt_lf_rq_ao1234_rt1_ec123_net1_f<br>
        ad_evt_lf_rq_ao1234_idl_ec123_net1_f<br>
        ad_evt_lf_rq_ao1234_hdr_ec123_net1_f<br>
        ad_evt_lf_rq_ao1234_sfr_ec123_net1_f<br>
      </td>
      <td>
        ad_evt_lf_rq_rw1234_ini_ec123_net1_f<br>
        ad_evt_lf_rq_rw1234_rt1_ec123_net1_f<br>
        ad_evt_lf_rq_rw1234_idl_ec123_net1_f<br>
        ad_evt_lf_rq_rw1234_hdr_ec123_net1_f<br>
        ad_evt_lf_rq_rw1234_sfr_ec123_net1_f<br>
      </td>
      <td>
        ad_evt_lf_rq_fa1234_ini_gamePlay_ec123_net1_f<br>
        ad_evt_lf_rq_fa1234_rt1_gamePlay_ec123_net1_f<br>
        ad_evt_lf_rq_fa1234_idl_gamePlay_ec123_net1_f<br>
        ad_evt_lf_rq_fa1234_hdr_gamePlay_ec123_net1_f<br>
        ad_evt_lf_rq_fa1234_sfr_gamePlay_ec123_net1_f<br>
      </td>
      <td>
        ad_evt_lf_rq_pu1234_ini_naFull_ec123_net1<br>
      </td>
      <td>
        ad_evt_lf_rq_bn1234_ini_fb_ec123_net1<br>
      </td>
      <td>
        ad_evt_lf_rq_mr1234_ini_ec123_net1<br>
      </td>
      <td>
        ad_evt_lf_rq_cl1234_ini_ec123_net1<br>
      </td>
    </tr>
    <tr>
      <th>19</th>
      <td>13</td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
    </tr>
    <tr>
      <th>20</th>
      <td>14</td>
      <td>Group</td>
      <td>Displayed</td>
      <td>ad_evt_dsp_sh_ao1234_apLaunc<br></td>
      <td>ad_evt_dsp_sh_ao1234_apResum<br></td>
      <td>ad_evt_dsp_sh_rw1234_x2coin<br></td>
      <td>ad_evt_dsp_sh_fa1234_homegate<br></td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
    </tr>
    <tr>
      <th>21</th>
      <td>15</td>
      <td>Group</td>
      <td>Clicked</td>
      <td>ad_evt_clk_sh_ao1234_apLaunc<br></td>
      <td>ad_evt_clk_sh_ao1234_apResum<br></td>
      <td>ad_evt_clk_sh_rw1234_x2coin<br></td>
      <td>ad_evt_clk_sh_fa1234_homegate<br></td>
      <td>ad_evt_clk_sh_pu1234_offerwall<br></td>
      <td>ad_evt_clk_sh_bn1234_fb<br></td>
      <td>ad_evt_clk_sh_mr1234_mrDef<br></td>
      <td>ad_evt_clk_sh_cl1234_clDef<br></td>
    </tr>
    <tr>
      <th>22</th>
      <td>16</td>
      <td>Group</td>
      <td>Impression</td>
      <td>ad_evt_imp_sh_ao1234_apLaunc<br></td>
      <td>ad_evt_imp_sh_ao1234_apResum<br></td>
      <td>ad_evt_imp_sh_rw1234_x2coin<br></td>
      <td>ad_evt_imp_sh_fa1234_homegate<br></td>
      <td>ad_evt_imp_sh_pu1234_offerwall<br></td>
      <td>ad_evt_imp_sh_bn1234_fb<br></td>
      <td>ad_evt_imp_sh_mr1234_mrDef<br></td>
      <td>ad_evt_imp_sh_cl1234_clDef<br></td>
    </tr>
    <tr>
      <th>23</th>
      <td>17</td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
    </tr>
    <tr>
      <th>24</th>
      <td>18</td>
      <td>Group</td>
      <td>Closed</td>
      <td>ad_evt_cls_sh_ao1234_apLaunc<br></td>
      <td>ad_evt_cls_sh_ao1234_apResum<br></td>
      <td>ad_evt_cls_sh_rw1234_x2coin<br></td>
      <td>ad_evt_cls_sh_fa1234_homegate<br></td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
    </tr>
    <tr>
      <th>25</th>
      <td>19</td>
      <td>Group</td>
      <td>Rewarded</td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
      <td>ad_evt_rwd_sh_rw1234_x2coin<br></td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
    </tr>
    <tr>
      <th>26</th>
      <td>20</td>
      <td>Group</td>
      <td>ShowFailed</td>
      <td>ad_evt_shf_sh_ao1234_apLaunc_ec123_net1<br></td>
      <td>ad_evt_shf_sh_ao1234_apResum_ec123_net1<br></td>
      <td>ad_evt_shf_sh_rw1234_x2coin_ec123_net1<br></td>
      <td>ad_evt_shf_sh_fa1234_homegate_ec123_net1<br></td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
    </tr>
    <tr>
      <th>27</th>
      <td>&nbsp;</td>
      <td>Legend</td>
      <td>Giải nghĩa</td>
      <td>
        <code>param1</code> dùng ví dụ <code>ao1234</code> là group id của ad đang callback.<br>
        Event load của channel này có thể mang các context <code>ini / rtN / idl / hdr / sfr</code>.<br>
        Event sau show dùng <code>lastPos = apLaunc</code>.<br>
        Riêng <code>shf</code> thêm hậu tố <code>ec{code}_net{0|1}</code> hoặc <code>noec_net{0|1}</code>.
      </td>
      <td>
        <code>param1</code> dùng ví dụ <code>ao1234</code> là group id của ad đang callback.<br>
        Event load của channel này có thể mang các context <code>ini / rtN / idl / hdr / sfr</code>.<br>
        Event sau show dùng <code>lastPos = apResum</code>.<br>
        Riêng <code>shf</code> thêm hậu tố <code>ec{code}_net{0|1}</code> hoặc <code>noec_net{0|1}</code>.
      </td>
      <td>
        <code>param1</code> dùng ví dụ <code>rw1234</code> là group id của ad đang callback.<br>
        Event load của channel này có thể mang các context <code>ini / rtN / idl / hdr / sfr</code>.<br>
        Event sau show dùng ví dụ <code>lastPos = x2coin</code> vì pos được truyền từ caller.<br>
        Riêng <code>shf</code> thêm hậu tố <code>ec{code}_net{0|1}</code> hoặc <code>noec_net{0|1}</code>.
      </td>
      <td>
        <code>param1</code> dùng ví dụ <code>fa1234</code> là group id của ad đang callback.<br>
        Event load của channel này có thể mang các context <code>ini / rtN / idl / hdr / sfr</code>.<br>
        Event sau show dùng ví dụ <code>lastPos = homegate</code>.<br>
        Riêng <code>shf</code> thêm hậu tố <code>ec{code}_net{0|1}</code> hoặc <code>noec_net{0|1}</code>.
      </td>
      <td>
        <code>param1</code> dùng ví dụ <code>pu1234</code> là group id của ad đang callback.<br>
        Event load của channel này hiện chỉ có context <code>ini</code>.<br>
        Event sau show dùng ví dụ <code>lastPos = offerwall</code>.
      </td>
      <td>
        <code>param1</code> dùng ví dụ <code>bn1234</code> là group id của ad đang callback.<br>
        Event load của channel này mang thêm placement token như <code>fb/ft/tl/tr/bl/br</code>.<br>
        Event sau show dùng <code>lastPos = fb</code> hoặc token placement tương ứng.
      </td>
      <td>
        <code>param1</code> dùng ví dụ <code>mr1234</code> là group id của ad đang callback.<br>
        Event load của channel này hiện chỉ có context <code>ini</code>.<br>
        Event sau show dùng <code>lastPos = mrDef</code>.
      </td>
      <td>
        <code>param1</code> dùng ví dụ <code>cl1234</code> là group id của ad đang callback.<br>
        Event load của channel này hiện chỉ có context <code>ini</code>.<br>
        Event sau show dùng <code>lastPos = clDef</code>.
      </td>
    </tr>
    <tr>
      <th>28</th>
      <td colspan="11">&nbsp;</td>
    </tr>
    <tr>
      <th>29</th>
      <td>21</td>
      <td>System</td>
      <td>Activate</td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
      <td>
        <strong>ad_sy_bn_act_fb</strong><br>
        <br>
        Drop =&gt;<br>
        ad_sy_bn_act_fb_n_iap<br>
        ad_sy_bn_act_fb_n_cfg<br>
      </td>
      <td>
        <strong>ad_sy_mr_act_mrDef</strong><br>
        <br>
        Drop =&gt;<br>
        ad_sy_mr_act_mrDef_n_iap<br>
        ad_sy_mr_act_mrDef_n_cfg<br>
      </td>
      <td>&nbsp;</td>
    </tr>
    <tr>
      <th>30</th>
      <td>22</td>
      <td>AdCore</td>
      <td>Activate</td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
      <td>
        <br>
        Drop =&gt;<br>
        ad_ac_bn_act_fb_n_group<br>
      </td>
      <td>
        <br>
        Drop =&gt;<br>
        ad_ac_mr_act_mrDef_n_group<br>
      </td>
      <td>&nbsp;</td>
    </tr>
    <tr>
      <th>31</th>
      <td>23</td>
      <td>Group</td>
      <td>Activate</td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
      <td>
        <br>
        Drop =&gt;<br>
        ad_gr_act_bn1234_fb_n_nready<br>
        ad_gr_act_bn1234_fb_n_showing<br>
        <br>
        API =&gt;<br>
        <strong>ad_gr_act_bn1234_fb_api</strong><br>
      </td>
      <td>
        <br>
        Drop =&gt;<br>
        ad_gr_act_mr1234_mrDef_n_nready<br>
        ad_gr_act_mr1234_mrDef_n_showing<br>
        <br>
        API =&gt;<br>
        <strong>ad_gr_act_mr1234_mrDef_api</strong><br>
      </td>
      <td>&nbsp;</td>
    </tr>
    <tr>
      <th>32</th>
      <td>&nbsp;</td>
      <td>Legend</td>
      <td>Giải nghĩa</td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
      <td>
        Flow <code>activate</code> của <code>Banner</code> dùng placement token như <code>fb/ft/tl/tr/bl/br</code>.<br>
        Flow này tách riêng với <code>show</code> đến tận lúc chạm API ở tầng <code>group</code>.<br>
        Sau điểm merge trong group, callback event dùng chung với flow <code>show</code>.<br>
        Có thể filter API bằng mẫu <code>ad_gr_act_bn1234_fb_api</code>.
      </td>
      <td>
        Flow <code>activate</code> chỉ có ở <code>Mrec</code> và không nhận pos từ caller nên dùng <code>mrDef</code>.<br>
        Flow này tách riêng với <code>show</code> đến tận lúc chạm API ở tầng <code>group</code>.<br>
        Sau điểm merge trong group, callback event dùng chung với flow <code>show</code>.<br>
        Có thể filter API bằng mẫu <code>ad_gr_act_mr1234_mrDef_api</code>.
      </td>
      <td>&nbsp;</td>
    </tr>
    <tr>
      <th>33</th>
      <td>24</td>
      <td>AdCore</td>
      <td>Hide</td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
      <td>
        <br>
        Drop =&gt;<br>
        ad_ac_pu_hid_offerwall_n_group<br>
      </td>
      <td>
        <br>
        Drop =&gt;<br>
        ad_ac_bn_hid_fb_n_group<br>
      </td>
      <td>
        <br>
        Drop =&gt;<br>
        ad_ac_mr_hid_mrDef_n_group<br>
      </td>
      <td>
        <br>
        Drop =&gt;<br>
        ad_ac_cl_hid_clDef_n_group<br>
      </td>
    </tr>
    <tr>
      <th>34</th>
      <td>25</td>
      <td>Group</td>
      <td>Hide</td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
      <td>
        <br>
        Drop =&gt;<br>
        ad_gr_hid_pu1234_offerwall_n_nshowing<br>
        <br>
        API =&gt;<br>
        <strong>ad_gr_hid_pu1234_offerwall_api</strong><br>
      </td>
      <td>
        <br>
        Drop =&gt;<br>
        ad_gr_hid_bn1234_fb_n_nshowing<br>
        <br>
        API =&gt;<br>
        <strong>ad_gr_hid_bn1234_fb_api</strong><br>
      </td>
      <td>
        <br>
        Drop =&gt;<br>
        ad_gr_hid_mr1234_mrDef_n_nshowing<br>
        <br>
        API =&gt;<br>
        <strong>ad_gr_hid_mr1234_mrDef_api</strong><br>
      </td>
      <td>
        <br>
        Drop =&gt;<br>
        ad_gr_hid_cl1234_clDef_n_nshowing<br>
        <br>
        API =&gt;<br>
        <strong>ad_gr_hid_cl1234_clDef_api</strong><br>
      </td>
    </tr>
    <tr>
      <th>35</th>
      <td>&nbsp;</td>
      <td>Legend</td>
      <td>Giải nghĩa</td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
      <td>&nbsp;</td>
      <td>
        Flow <code>hide</code> của <code>Popup</code> bắt đầu tracking từ tầng <code>system</code> với <code>ad_sy_pu_hid_offerwall</code>.<br>
        Flow này dùng <code>pos = offerwall</code> và group dùng <code>groupId = pu1234</code>.<br>
        Có thể filter API bằng mẫu <code>ad_gr_hid_pu1234_offerwall_api</code>.
      </td>
      <td>
        Flow <code>hide</code> của <code>Banner</code> bắt đầu tracking từ tầng <code>system</code> với <code>ad_sy_bn_hid_fb</code>.<br>
        Flow này dùng placement token như <code>fb/ft/tl/tr/bl/br</code>.<br>
        Có thể filter API bằng mẫu <code>ad_gr_hid_bn1234_fb_api</code>.
      </td>
      <td>
        Flow <code>hide</code> của <code>Mrec</code> bắt đầu tracking từ tầng <code>system</code> với <code>ad_sy_mr_hid_mrDef</code>.<br>
        Flow này không nhận pos từ caller nên dùng <code>mrDef</code>.<br>
        Có thể filter API bằng mẫu <code>ad_gr_hid_mr1234_mrDef_api</code>.
      </td>
      <td>
        Flow <code>hide</code> của <code>Collap</code> bắt đầu tracking từ tầng <code>system</code> với <code>ad_sy_cl_hid_clDef</code>.<br>
        Flow này không nhận pos từ caller nên dùng <code>clDef</code>.<br>
        Có thể filter API bằng mẫu <code>ad_gr_hid_cl1234_clDef_api</code>.
      </td>
    </tr>
  </tbody>
</table>

## Notes

### AppLaunch

- Nếu `ComebackChannel.LaunchAdType = AO` và `AppOpenUnit.UseBackup = true`, `AdCore_MainAndroid` có thể mở thêm backup AppOpen mediation theo thứ tự tuần tự khi load/show fail.
- Vì vậy, một lần `ad_sy_al_rq` có thể dẫn tới nhiều branch `ad_gr_al_rq_{groupId}_{context}_*` của các AppOpen group khác nhau.
- Nếu `UseBackup = true` mà tới lúc show không có candidate AppOpen nào ready, wrapper sẽ emit `ad_gr_sh_apLaunc_n_nready` chỉ theo `pos`, không gắn `groupId`.
- Nếu có candidate ready, `Show` vẫn gắn vào AppOpen group thực sự được chọn ở thời điểm call.

#### D2 - System / AutoShow

- `ad_atsy_al_sh_apLaunc`: tracking đầu vào của luồng auto-show ở tầng system.
- `ad_atsy_al_sh_apLaunc_n_tmo`: drop do timeout trước khi call được xuống adcore.
- `ad_atsy_al_sh_apLaunc_n_iap`: drop do user đã mua Remove Ads, tương ứng `AdsLogic.IsRemovedAd = true`.
- `ad_atsy_al_sh_apLaunc_n_cfg`: drop do `configs == null` hoặc `configs.IsEnabled = false`.
- `ad_atsy_al_sh_apLaunc_n_ign`: drop do bị ignore ở system nên không đi tiếp.

#### D3 - System / Show

- `D3` để trống: `AppLaunch` sau khi pass `auto-show` ở system sẽ đi thẳng xuống `adcore`, không còn tầng `show system` riêng.

#### D4 - AdCore / Show

- `ad_ac_al_sh_apLaunc_n_group`: drop do không resolve được group để đi tiếp xuống tầng `group`.

#### D5 - Group / Show

- `ad_gr_sh_ao1234_apLaunc_n_nready`: drop do ad chưa ready tại tầng `group` khi flow không dùng backup hoặc wrapper đã delegate xuống một AppOpen group cụ thể.
- `ad_gr_sh_apLaunc_n_nready`: drop do `AppOpen` đang chạy backup flow, nhưng tại thời điểm show không có candidate nào ready nên tracking chỉ giữ `pos`.
- `ad_gr_sh_ao1234_apLaunc_api`: tracking khi flow `show` chạm API ở tầng `group`.

#### D8 - System / Request

- `ad_sy_al_rq`: tracking đầu vào của flow `request` tại tầng `system`.
- `ad_sy_al_rq_n_iap`: drop do user đã mua Remove Ads, tương ứng `AdsLogic.IsRemovedAd = true`.
- `ad_sy_al_rq_n_cfg`: drop do `configs == null` hoặc `configs.IsEnabled = false`.

#### D9 - AdCore / Request

- `ad_ac_al_rq_n_group`: drop do không resolve được group để đi tiếp xuống tầng `group`.

#### D10 - Group / Init

- `ad_gr_al_rq_ao1234_ini_n_inited`: drop do group đã được initialize trước đó.
- `ad_gr_al_rq_ao1234_ini_n_noid`: drop do thiếu ad unit id ở group.
- `ad_gr_al_rq_ao1234_ini_api`: tracking khi flow `request` chạm API ở context `ini`.

#### D11 - Group / Retry

- `ad_gr_al_rq_ao1234_rt1_n_gate`: drop do retry gate không cho phép schedule retry đầu tiên.
- `ad_gr_al_rq_ao1234_rt1_n_host`: drop do không có host để chạy coroutine retry đầu tiên.
- `ad_gr_al_rq_ao1234_rt1_n_dis`: drop do reload bị chặn bởi `DisablePostInitReload`.
- `ad_gr_al_rq_ao1234_rt1_n_aload`: drop do ad đã ở trạng thái loaded khi retry bắt đầu.
- `ad_gr_al_rq_ao1234_rt1_n_loading`: drop do group đang loading.
- `ad_gr_al_rq_ao1234_rt1_api`: tracking khi flow `request` chạm API ở context `rtN`, ví dụ retry lần 1.

#### D12 - Group / Idle

- `ad_gr_al_rq_ao1234_idl_n_retry`: drop do retry logic đang ở trạng thái waiting nên self-heal idle không chạy tiếp.
- `ad_gr_al_rq_ao1234_idl_n_dis`: drop do reload bị chặn bởi `DisablePostInitReload`.
- `ad_gr_al_rq_ao1234_idl_n_aload`: drop do ad đã ở trạng thái loaded khi idle reload.
- `ad_gr_al_rq_ao1234_idl_n_loading`: drop do group đang loading.
- `ad_gr_al_rq_ao1234_idl_api`: tracking khi flow `request` chạm API ở context `idl`.

#### D13 - Group / HideReload

- `ad_gr_al_rq_ao1234_hdr_n_budget`: drop do budget show đã cạn ở bước hide reload.
- `ad_gr_al_rq_ao1234_hdr_n_dis`: drop do reload bị chặn bởi `DisablePostInitReload`.
- `ad_gr_al_rq_ao1234_hdr_n_aload`: drop do ad đã ở trạng thái loaded khi hide reload bắt đầu.
- `ad_gr_al_rq_ao1234_hdr_n_loading`: drop do group đang loading.
- `ad_gr_al_rq_ao1234_hdr_api`: tracking khi flow `request` chạm API ở context `hdr`.

#### D14 - Group / DisplayFailReload

- `ad_gr_al_rq_ao1234_sfr_n_dis`: drop do reload bị chặn bởi `DisablePostInitReload`.
- `ad_gr_al_rq_ao1234_sfr_n_aload`: drop do ad đã ở trạng thái loaded khi display fail reload bắt đầu.
- `ad_gr_al_rq_ao1234_sfr_n_loading`: drop do group đang loading.
- `ad_gr_al_rq_ao1234_sfr_api`: tracking khi flow `request` chạm API ở context `sfr`.

### AppResume

- Nếu `ComebackChannel.ResumeAdType = AO` và `AppOpenUnit.UseBackup = true`, `AdCore_MainAndroid` có thể dùng cùng shared AppOpen fallback chain với `AppLaunch`.
- Vì vậy, một lần `ad_sy_ar_rq` có thể dẫn tới nhiều branch `ad_gr_ar_rq_{groupId}_{context}_*` của các AppOpen group khác nhau khi backup mediation được mở thêm.
- Nếu `UseBackup = true` mà tới lúc auto-show không có candidate AppOpen nào ready, wrapper sẽ emit `ad_gr_sh_apResum_n_nready` chỉ theo `pos`, không gắn `groupId`.
- Nếu có candidate ready, `Show` vẫn gắn với AppOpen group thực sự được chọn ở thời điểm auto-show.

#### E2 - System / AutoShow

- `ad_atsy_ar_sh_apResum`: tracking đầu vào của luồng auto-show khi resume sau lần mở app đầu tiên.
- `ad_atsy_ar_sh_apResum_n_iap`: drop do user đã mua Remove Ads, tương ứng `AdsLogic.IsRemovedAd = true`.
- `ad_atsy_ar_sh_apResum_n_cfg`: drop do `configs == null` hoặc `configs.IsEnabled = false`.
- `ad_atsy_ar_sh_apResum_n_ign`: drop do bị ignore ở system.
- `ad_atsy_ar_sh_apResum_n_fsblk`: drop do bị chặn bởi fullscreen ad vừa diễn ra.
- `ad_atsy_ar_sh_apResum_n_rcfg`: drop do remote config chưa sẵn sàng.
- `ad_atsy_ar_sh_apResum_n_nready`: drop do ad chưa ready ở auto-show system.

### ForceAd

#### G2 - System / AutoShow

- `ad_atsy_fa_sh_homegate`: tracking đầu vào của luồng auto-show BreakAd ở tầng system với pos ví dụ `homegate`.
- `ad_atsy_fa_sh_homegate_n_iap`: drop do user đã mua Remove Ads, tương ứng `AdsLogic.IsRemovedAd = true`.
- `ad_atsy_fa_sh_homegate_n_null`: drop do thiếu dữ liệu cần thiết hoặc resolve ra `null`, ví dụ `configs == null`, `BreakAdConfig == null`, `BreakPos` rỗng, `posInfo == null`, hoặc `group` rỗng.
- `ad_atsy_fa_sh_homegate_n_block`: drop do flow `BreakAd` bị gate chặn, ví dụ `configs.IsEnabled = false`, `BreakAdConfig.IsEnabled = false`, hoặc `posInfo.CanShow = false`.
- `ad_atsy_fa_sh_homegate_n_ign`: drop do bị ignore ở system.
- `ad_atsy_fa_sh_homegate_n_nready`: drop do group chưa ready trước khi call xuống adcore.

#### E4 - AdCore / Show

- `ad_ac_ar_sh_apResum_n_group`: drop do không resolve được group để đi tiếp xuống tầng `group`.

#### E5 - Group / Show

- `ad_gr_sh_ao1234_apResum_n_nready`: drop do ad chưa ready tại tầng `group` khi flow không dùng backup hoặc wrapper đã delegate xuống một AppOpen group cụ thể.
- `ad_gr_sh_apResum_n_nready`: drop do `AppOpen` đang chạy backup flow, nhưng tại thời điểm show không có candidate nào ready nên tracking chỉ giữ `pos`.
- `ad_gr_sh_ao1234_apResum_api`: tracking khi flow `show` chạm API ở tầng `group`.

#### E8 - System / Request

- `ad_sy_ar_rq`: tracking đầu vào của flow `request` tại tầng `system`.
- `ad_sy_ar_rq_n_iap`: drop do user đã mua Remove Ads, tương ứng `AdsLogic.IsRemovedAd = true`.
- `ad_sy_ar_rq_n_cfg`: drop do `configs == null` hoặc `configs.IsEnabled = false`.

#### E9 - AdCore / Request

- `ad_ac_ar_rq_n_group`: drop do không resolve được group để đi tiếp xuống tầng `group`.

#### E10 - Group / Init

- `ad_gr_ar_rq_ao1234_ini_n_inited`: drop do group đã được initialize trước đó.
- `ad_gr_ar_rq_ao1234_ini_n_noid`: drop do thiếu ad unit id ở group.
- `ad_gr_ar_rq_ao1234_ini_api`: tracking khi flow `request` chạm API ở context `ini`.

#### E11 - Group / Retry

- `ad_gr_ar_rq_ao1234_rt1_n_gate`: drop do retry gate không cho phép schedule retry đầu tiên.
- `ad_gr_ar_rq_ao1234_rt1_n_host`: drop do không có host để chạy coroutine retry đầu tiên.
- `ad_gr_ar_rq_ao1234_rt1_n_dis`: drop do reload bị chặn bởi `DisablePostInitReload`.
- `ad_gr_ar_rq_ao1234_rt1_n_aload`: drop do ad đã ở trạng thái loaded khi retry bắt đầu.
- `ad_gr_ar_rq_ao1234_rt1_n_loading`: drop do group đang loading.
- `ad_gr_ar_rq_ao1234_rt1_api`: tracking khi flow `request` chạm API ở context `rtN`, ví dụ retry lần 1.

#### E12 - Group / Idle

- `ad_gr_ar_rq_ao1234_idl_n_retry`: drop do retry logic đang ở trạng thái waiting nên self-heal idle không chạy tiếp.
- `ad_gr_ar_rq_ao1234_idl_n_dis`: drop do reload bị chặn bởi `DisablePostInitReload`.
- `ad_gr_ar_rq_ao1234_idl_n_aload`: drop do ad đã ở trạng thái loaded khi idle reload.
- `ad_gr_ar_rq_ao1234_idl_n_loading`: drop do group đang loading.
- `ad_gr_ar_rq_ao1234_idl_api`: tracking khi flow `request` chạm API ở context `idl`.

#### E13 - Group / HideReload

- `ad_gr_ar_rq_ao1234_hdr_n_budget`: drop do budget show đã cạn ở bước hide reload.
- `ad_gr_ar_rq_ao1234_hdr_n_dis`: drop do reload bị chặn bởi `DisablePostInitReload`.
- `ad_gr_ar_rq_ao1234_hdr_n_aload`: drop do ad đã ở trạng thái loaded khi hide reload bắt đầu.
- `ad_gr_ar_rq_ao1234_hdr_n_loading`: drop do group đang loading.
- `ad_gr_ar_rq_ao1234_hdr_api`: tracking khi flow `request` chạm API ở context `hdr`.

#### E14 - Group / DisplayFailReload

- `ad_gr_ar_rq_ao1234_sfr_n_dis`: drop do reload bị chặn bởi `DisablePostInitReload`.
- `ad_gr_ar_rq_ao1234_sfr_n_aload`: drop do ad đã ở trạng thái loaded khi display fail reload bắt đầu.
- `ad_gr_ar_rq_ao1234_sfr_n_loading`: drop do group đang loading.
- `ad_gr_ar_rq_ao1234_sfr_api`: tracking khi flow `request` chạm API ở context `sfr`.

### Rewarded

- `Rewarded` ở `AdCore_MainAndroid` có thể mở thêm backup mediation theo thứ tự tuần tự khi load/show fail.
- Vì vậy, một lần `ad_sy_rw_rq` có thể dẫn tới nhiều branch `ad_gr_rw_rq_{groupId}_{context}_*` của các rewarded group khác nhau.
- `Show` vẫn chỉ có một `ad_sy_rw_sh_{pos}`; group/api/callback sẽ gắn với rewarded group thực sự đang `ready` và được chọn để show ở thời điểm call.

#### F3 - System / Show

- `ad_sy_rw_sh_x2coin`: tracking đầu vào của flow `show` tại tầng `system` với pos ví dụ `x2coin`.
- `ad_sy_rw_sh_x2coin_n_cfg`: drop do `configs == null` hoặc `configs.IsEnabled = false`.
- `ad_sy_rw_sh_x2coin_n_ign`: drop do bị ignore ở bước show.
- `Rewarded` hiện chưa có nhánh `n_iap`, vì `IsDisable` trong code không check `AdsLogic.IsRemovedAd`.

#### F4 - AdCore / Show

- `ad_ac_rw_sh_x2coin_n_group`: drop do không resolve được group để đi tiếp xuống tầng `group`.

#### F5 - Group / Show

- `ad_gr_sh_rw1234_x2coin_n_nready`: drop do ad chưa ready tại tầng `group` trong flow thường hoặc khi `Rewarded` không dùng backup.
- `ad_gr_sh_x2coin_n_nready`: drop do `Rewarded` đang chạy backup flow, nhưng tại thời điểm show không có candidate nào ready nên tracking chỉ giữ `pos`, không gắn `groupId`.
- `ad_gr_sh_rw1234_x2coin_api`: tracking khi flow `show` chạm API ở tầng `group`.

#### F8 - System / Request

- `ad_sy_rw_rq`: tracking đầu vào của flow `request` tại tầng `system`.
- `ad_sy_rw_rq_n_cfg`: drop do `configs == null` hoặc `configs.IsEnabled = false`.
- `Rewarded` hiện chưa có nhánh `n_iap` ở flow `request`, vì `IsDisable` trong code không check `AdsLogic.IsRemovedAd`.

#### F9 - AdCore / Request

- `ad_ac_rw_rq_n_group`: drop do không resolve được group để đi tiếp xuống tầng `group`.

#### F10 - Group / Init

- `ad_gr_rw_rq_rw1234_ini_n_inited`: drop do group đã được initialize trước đó.
- `ad_gr_rw_rq_rw1234_ini_n_noid`: drop do thiếu ad unit id ở group.
- `ad_gr_rw_rq_rw1234_ini_api`: tracking khi flow `request` chạm API ở context `ini`.

#### F11 - Group / Retry

- `ad_gr_rw_rq_rw1234_rt1_n_gate`: drop do retry bị chặn bởi gate chung của retry scheduler.
- `ad_gr_rw_rq_rw1234_rt1_n_host`: drop do host không cho phép retry request ở thời điểm hiện tại.
- `ad_gr_rw_rq_rw1234_rt1_n_dis`: drop do reload bị chặn bởi `DisablePostInitReload`.
- `ad_gr_rw_rq_rw1234_rt1_n_aload`: drop do ad đã ở trạng thái loaded khi retry bắt đầu.
- `ad_gr_rw_rq_rw1234_rt1_n_loading`: drop do group đang loading.
- `ad_gr_rw_rq_rw1234_rt1_api`: tracking khi flow `request` chạm API ở context `rtN`, ví dụ retry lần 1.

#### F12 - Group / Idle

- `ad_gr_rw_rq_rw1234_idl_n_retry`: drop do đang ở thời gian chờ retry nên chưa được load lại.
- `ad_gr_rw_rq_rw1234_idl_n_dis`: drop do reload bị chặn bởi `DisablePostInitReload`.
- `ad_gr_rw_rq_rw1234_idl_n_aload`: drop do ad đã ở trạng thái loaded khi idle reload bắt đầu.
- `ad_gr_rw_rq_rw1234_idl_n_loading`: drop do group đang loading.
- `ad_gr_rw_rq_rw1234_idl_api`: tracking khi flow `request` chạm API ở context `idl`.

#### F13 - Group / HideReload

- `ad_gr_rw_rq_rw1234_hdr_n_budget`: drop do đã hết budget reload sau khi ad bị hide.
- `ad_gr_rw_rq_rw1234_hdr_n_dis`: drop do reload bị chặn bởi `DisablePostInitReload`.
- `ad_gr_rw_rq_rw1234_hdr_n_aload`: drop do ad đã ở trạng thái loaded khi hide reload bắt đầu.
- `ad_gr_rw_rq_rw1234_hdr_n_loading`: drop do group đang loading.
- `ad_gr_rw_rq_rw1234_hdr_api`: tracking khi flow `request` chạm API ở context `hdr`.

#### F14 - Group / DisplayFailReload

- `ad_gr_rw_rq_rw1234_sfr_n_dis`: drop do reload bị chặn bởi `DisablePostInitReload`.
- `ad_gr_rw_rq_rw1234_sfr_n_aload`: drop do ad đã ở trạng thái loaded khi display fail reload bắt đầu.
- `ad_gr_rw_rq_rw1234_sfr_n_loading`: drop do group đang loading.
- `ad_gr_rw_rq_rw1234_sfr_api`: tracking khi flow `request` chạm API ở context `sfr`.

### ForceAd

#### G3 - System / Show

- `ad_sy_fa_sh_homegate`: tracking đầu vào của flow `show` thường tại tầng `system` với pos ví dụ `homegate`.
- `ad_sy_fa_sh_homegate_n_iap`: drop do user đã mua Remove Ads, tương ứng `AdsLogic.IsRemovedAd = true`.
- `ad_sy_fa_sh_homegate_n_cfg`: drop do `configs == null` hoặc `configs.IsEnabled = false`.
- `ad_sy_fa_sh_homegate_n_pos`: drop do pos không tồn tại trong config hoặc pos đó bị tắt `CanShow`.
- `ad_sy_fa_sh_homegate_n_cap`: drop do chưa qua capping time để được show.
- `ad_sy_fa_sh_homegate_n_ign`: drop do bị ignore ở system.

#### G4 - AdCore / Show

- `ad_ac_fa_sh_homegate_n_group`: drop do không resolve được group từ pos để đi tiếp xuống tầng `group`.

#### G5 - Group / Show

- `ad_gr_sh_fa1234_homegate_n_nready`: drop do ad chưa ready tại tầng `group`.
- `ad_gr_sh_fa1234_homegate_api`: tracking khi flow `show` chạm API ở tầng `group`.

#### G8 - System / Request

- `ad_sy_fa_rq_gamePlay`: tracking đầu vào của flow `request` tại tầng `system`. `gamePlay` là ví dụ `groupName`.
- `ad_sy_fa_rq_gamePlay_n_iap`: drop do user đã mua Remove Ads, tương ứng `AdsLogic.IsRemovedAd = true`.
- `ad_sy_fa_rq_gamePlay_n_cfg`: drop do `configs == null` hoặc `configs.IsEnabled = false`.

#### G9 - AdCore / Request

- `ad_ac_fa_rq_gamePlay_n_group`: drop do không resolve được group để đi tiếp xuống tầng `group`.

#### G10 - Group / Init

- `ad_gr_fa_rq_fa1234_ini_n_inited_gamePlay`: drop do group đã được initialize trước đó.
- `ad_gr_fa_rq_fa1234_ini_n_noid_gamePlay`: drop do thiếu ad unit id ở group.
- `ad_gr_fa_rq_fa1234_ini_gamePlay_api`: tracking khi flow `request` chạm API ở context `ini`.
- Với `ForceAd`, `gamePlay` là ví dụ `groupName`, còn `fa1234` là ví dụ group id. Cụm filter ở tầng `group` có dạng `ad_gr_fa_rq_fa1234_{context}_gamePlay`.

#### G11 - Group / Retry

- `ad_gr_fa_rq_fa1234_rt1_n_gate_gamePlay`: drop do retry bị chặn bởi gate chung của retry scheduler.
- `ad_gr_fa_rq_fa1234_rt1_n_host_gamePlay`: drop do host không cho phép retry request ở thời điểm hiện tại.
- `ad_gr_fa_rq_fa1234_rt1_n_dis_gamePlay`: drop do reload bị chặn bởi `DisablePostInitReload`.
- `ad_gr_fa_rq_fa1234_rt1_n_aload_gamePlay`: drop do ad đã ở trạng thái loaded khi retry bắt đầu.
- `ad_gr_fa_rq_fa1234_rt1_n_loading_gamePlay`: drop do group đang loading.
- `ad_gr_fa_rq_fa1234_rt1_gamePlay_api`: tracking khi flow `request` chạm API ở context `rtN`, ví dụ retry lần 1.

#### G12 - Group / Idle

- `ad_gr_fa_rq_fa1234_idl_n_retry_gamePlay`: drop do đang ở thời gian chờ retry nên chưa được load lại.
- `ad_gr_fa_rq_fa1234_idl_n_dis_gamePlay`: drop do reload bị chặn bởi `DisablePostInitReload`.
- `ad_gr_fa_rq_fa1234_idl_n_aload_gamePlay`: drop do ad đã ở trạng thái loaded khi idle reload bắt đầu.
- `ad_gr_fa_rq_fa1234_idl_n_loading_gamePlay`: drop do group đang loading.
- `ad_gr_fa_rq_fa1234_idl_gamePlay_api`: tracking khi flow `request` chạm API ở context `idl`.

#### G13 - Group / HideReload

- `ad_gr_fa_rq_fa1234_hdr_n_budget_gamePlay`: drop do đã hết budget reload sau khi ad bị hide.
- `ad_gr_fa_rq_fa1234_hdr_n_dis_gamePlay`: drop do reload bị chặn bởi `DisablePostInitReload`.
- `ad_gr_fa_rq_fa1234_hdr_n_aload_gamePlay`: drop do ad đã ở trạng thái loaded khi hide reload bắt đầu.
- `ad_gr_fa_rq_fa1234_hdr_n_loading_gamePlay`: drop do group đang loading.
- `ad_gr_fa_rq_fa1234_hdr_gamePlay_api`: tracking khi flow `request` chạm API ở context `hdr`.

#### G14 - Group / DisplayFailReload

- `ad_gr_fa_rq_fa1234_sfr_n_dis_gamePlay`: drop do reload bị chặn bởi `DisablePostInitReload`.
- `ad_gr_fa_rq_fa1234_sfr_n_aload_gamePlay`: drop do ad đã ở trạng thái loaded khi display fail reload bắt đầu.
- `ad_gr_fa_rq_fa1234_sfr_n_loading_gamePlay`: drop do group đang loading.
- `ad_gr_fa_rq_fa1234_sfr_gamePlay_api`: tracking khi flow `request` chạm API ở context `sfr`.

### Popup

#### H3 - System / Show

- `ad_sy_pu_sh_offerwall`: tracking đầu vào của flow `show` tại tầng `system` với pos ví dụ `offerwall`.
- `ad_sy_pu_sh_offerwall_n_iap`: drop do user đã mua Remove Ads, tương ứng `AdsLogic.IsRemovedAd = true`.
- `ad_sy_pu_sh_offerwall_n_cfg`: drop do `configs == null` hoặc `configs.IsEnabled = false`.
- `ad_sy_pu_sh_offerwall_n_pos`: drop do pos không tồn tại trong config hoặc pos đó đang bị tắt.

#### H4 - AdCore / Show

- `ad_ac_pu_sh_offerwall_n_group`: drop do không resolve được group từ pos để đi tiếp xuống tầng `group`.

#### H5 - Group / Show

- `ad_gr_sh_pu1234_offerwall_n_nready`: drop do ad chưa ready tại tầng `group`.
- `ad_gr_sh_pu1234_offerwall_n_showing`: drop do popup đang ở trạng thái showing.
- `ad_gr_sh_pu1234_offerwall_n_upos`: drop do popup chưa được gọi `UpdatePos`, nên chưa có layout runtime để show.
- `ad_gr_sh_pu1234_offerwall_n_size0`: drop do popup đã `UpdatePos` nhưng width/height hiện tại không hợp lệ, đang `<= 0`.
- `ad_gr_sh_pu1234_offerwall_api`: tracking khi flow `show` chạm API ở tầng `group`.

#### H8 - System / Request

- `ad_sy_pu_rq_naFull`: tracking đầu vào của flow `request` tại tầng `system`. `naFull` là ví dụ `groupName` của `Popup`.
- `ad_sy_pu_rq_naFull_n_iap`: drop do user đã mua Remove Ads, tương ứng `AdsLogic.IsRemovedAd = true`.
- `ad_sy_pu_rq_naFull_n_cfg`: drop do `configs == null` hoặc `configs.IsEnabled = false`.

#### H9 - AdCore / Request

- `ad_ac_pu_rq_naFull_n_group`: drop do không resolve được group để đi tiếp xuống tầng `group`.

#### H10 - Group / Init

- `ad_gr_pu_rq_pu1234_ini_n_inited_naFull`: drop do group đã được initialize trước đó.
- `ad_gr_pu_rq_pu1234_ini_n_noid_naFull`: drop do thiếu ad unit id ở group.
- `ad_gr_pu_rq_pu1234_ini_n_aload_naFull`: drop do ad đã ở trạng thái loaded ngay trong bước init request.
- `ad_gr_pu_rq_pu1234_ini_naFull_api`: tracking khi flow `request` chạm API ở context `ini`.
- Với `Popup`, hiện request chỉ có context `ini` ở `RectGroup`, chưa có `rtN/idl/hdr/sfr` như các FS group.

#### H33 - AdCore / Hide

- `ad_sy_pu_hid_offerwall`: system entry of hide flow.
- `ad_ac_pu_hid_offerwall_n_group`: drop do không resolve được group để đi tiếp xuống tầng `group`.

#### H34 - Group / Hide

- `ad_gr_hid_pu1234_offerwall_n_nshowing`: drop do popup hiện không ở trạng thái showing nên không có gì để hide.
- `ad_gr_hid_pu1234_offerwall_api`: tracking khi flow `hide` chạm API ở tầng `group`.

### Banner

#### I3 - System / Show

- `ad_sy_bn_sh_fb`: tracking đầu vào của flow `show` tại tầng `system`.
- `ad_sy_bn_sh_fb_n_iap`: drop do user đã mua Remove Ads, tương ứng `AdsLogic.IsRemovedAd = true`.
- `ad_sy_bn_sh_fb_n_cfg`: drop do `configs == null` hoặc `configs.IsEnabled = false`.

#### I4 - AdCore / Show

- `ad_ac_bn_sh_fb_n_group`: drop do không resolve được group để đi tiếp xuống tầng `group`.

#### I5 - Group / Show

- `ad_gr_sh_bn1234_fb_n_nready`: drop do ad chưa ready tại tầng `group`.
- `ad_gr_sh_bn1234_fb_n_showing`: drop do banner đang ở trạng thái showing.
- `ad_gr_sh_bn1234_fb_api`: tracking khi flow `show` chạm API ở tầng `group`.

#### I8 - System / Request

- `ad_sy_bn_rq_fb`: tracking đầu vào của flow `request` tại tầng `system`.
- `ad_sy_bn_rq_fb_n_iap`: drop do user đã mua Remove Ads, tương ứng `AdsLogic.IsRemovedAd = true`.
- `ad_sy_bn_rq_fb_n_cfg`: drop do `configs == null` hoặc `configs.IsEnabled = false`.

#### I9 - AdCore / Request

- `ad_ac_bn_rq_fb_n_group`: drop do không resolve được group để đi tiếp xuống tầng `group`.

#### I10 - Group / Init

- `ad_gr_bn_rq_bn1234_ini_n_inited_fb`: drop do group đã được initialize trước đó.
- `ad_gr_bn_rq_bn1234_ini_n_noid_fb`: drop do thiếu ad unit id ở group.
- `ad_gr_bn_rq_bn1234_ini_n_aload_fb`: drop do ad đã ở trạng thái loaded ngay trong bước init request.
- `ad_gr_bn_rq_bn1234_ini_fb_api`: tracking khi flow `request` chạm API ở context `ini`.
- Với `Banner`, luồng init không đi qua `groupName`, nên request ở tầng `group` giữ `groupId = bn1234` và placement token như `fb`.

#### I29 - System / Activate

- `ad_sy_bn_act_fb`: tracking đầu vào của flow `activate` tại tầng `system`.
- `ad_sy_bn_act_fb_n_iap`: drop do user đã mua Remove Ads, tương ứng `AdsLogic.IsRemovedAd = true`.
- `ad_sy_bn_act_fb_n_cfg`: drop do `configs == null` hoặc `configs.IsEnabled = false`.

#### I30 - AdCore / Activate

- `ad_ac_bn_act_fb_n_group`: drop do không resolve được group để đi tiếp xuống tầng `group`.

#### I31 - Group / Activate

- `ad_gr_act_bn1234_fb_n_nready`: drop do group chưa init xong nên chưa thể activate view.
- `ad_gr_act_bn1234_fb_n_showing`: drop do banner đang ở trạng thái showing.
- `ad_gr_act_bn1234_fb_api`: tracking khi flow `activate` chạm API ở tầng `group`.
- Flow `activate` tách riêng với `show` đến tận điểm `api`; callback thật sau đó chỉ giữ các event SDK như `clk / imp`.

#### I33 - AdCore / Hide

- `ad_sy_bn_hid_fb`: system entry of hide flow.
- `ad_ac_bn_hid_fb_n_group`: drop do không resolve được group để đi tiếp xuống tầng `group`.

#### I34 - Group / Hide

- `ad_gr_hid_bn1234_fb_n_nshowing`: drop do banner hiện không ở trạng thái showing nên không có gì để hide.
- `ad_gr_hid_bn1234_fb_api`: tracking khi flow `hide` chạm API ở tầng `group`.

### Mrec

- Nếu `MrecUnit.UseBackup = true`, `AdCore_MainAndroid` có thể mở thêm backup mediation theo thứ tự tuần tự khi load fail.
- Vì vậy, một lần `ad_sy_mr_rq` có thể dẫn tới nhiều branch `ad_gr_mr_rq_{groupId}_{context}_*` của các mrec group khác nhau.
- `show/activate/hide` vẫn gắn với mrec group thực sự đang loaded hoặc đang được dùng ở thời điểm call; grammar tracking không đổi.

#### J3 - System / Show

- `ad_sy_mr_sh_mrDef`: tracking đầu vào của flow `show` tại tầng `system`.
- `ad_sy_mr_sh_mrDef_n_iap`: drop do user đã mua Remove Ads, tương ứng `AdsLogic.IsRemovedAd = true`.
- `ad_sy_mr_sh_mrDef_n_cfg`: drop do `configs == null` hoặc `configs.IsEnabled = false`.

#### J4 - AdCore / Show

- `ad_ac_mr_sh_mrDef_n_group`: drop do không resolve được group để đi tiếp xuống tầng `group`.

#### J5 - Group / Show

- `ad_gr_sh_mr1234_mrDef_n_nready`: drop do ad chưa ready tại tầng `group`.
- `ad_gr_sh_mr1234_mrDef_n_showing`: drop do mrec đang ở trạng thái showing.
- `ad_gr_sh_mr1234_mrDef_api`: tracking khi flow `show` chạm API ở tầng `group`.

#### J8 - System / Request

- `ad_sy_mr_rq`: tracking đầu vào của flow `request` tại tầng `system`.
- `ad_sy_mr_rq_n_iap`: drop do user đã mua Remove Ads, tương ứng `AdsLogic.IsRemovedAd = true`.
- `ad_sy_mr_rq_n_cfg`: drop do `configs == null` hoặc `configs.IsEnabled = false`.

#### J9 - AdCore / Request

- `ad_ac_mr_rq_n_group`: drop do không resolve được group để đi tiếp xuống tầng `group`.

#### J10 - Group / Init

- `ad_gr_mr_rq_mr1234_ini_n_inited`: drop do group đã được initialize trước đó.
- `ad_gr_mr_rq_mr1234_ini_n_noid`: drop do thiếu ad unit id ở group.
- `ad_gr_mr_rq_mr1234_ini_n_aload`: drop do ad đã ở trạng thái loaded ngay trong bước init request.
- `ad_gr_mr_rq_mr1234_ini_api`: tracking khi flow `request` chạm API ở context `ini`.
- Với `Mrec`, luồng init không đi qua `groupName`, nên request ở tầng `group` chỉ giữ `groupId = mr1234`.

#### J29 - System / Activate

- `ad_sy_mr_act_mrDef`: tracking đầu vào của flow `activate` tại tầng `system`.
- `ad_sy_mr_act_mrDef_n_iap`: drop do user đã mua Remove Ads, tương ứng `AdsLogic.IsRemovedAd = true`.
- `ad_sy_mr_act_mrDef_n_cfg`: drop do `configs == null` hoặc `configs.IsEnabled = false`.

#### J30 - AdCore / Activate

- `ad_ac_mr_act_mrDef_n_group`: drop do không resolve được group để đi tiếp xuống tầng `group`.

#### J31 - Group / Activate

- `ad_gr_act_mr1234_mrDef_n_nready`: drop do group chưa init xong nên chưa thể activate view.
- `ad_gr_act_mr1234_mrDef_n_showing`: drop do mrec đang ở trạng thái showing.
- `ad_gr_act_mr1234_mrDef_api`: tracking khi flow `activate` chạm API ở tầng `group`.
- Flow `activate` tách riêng với `show` đến tận điểm `api`; callback thật sau đó chỉ giữ các event SDK như `clk / imp`.

#### J33 - AdCore / Hide

- `ad_sy_mr_hid_mrDef`: system entry of hide flow.
- `ad_ac_mr_hid_mrDef_n_group`: drop do không resolve được group để đi tiếp xuống tầng `group`.

#### J34 - Group / Hide

- `ad_gr_hid_mr1234_mrDef_n_nshowing`: drop do mrec hiện không ở trạng thái showing nên không có gì để hide.
- `ad_gr_hid_mr1234_mrDef_api`: tracking khi flow `hide` chạm API ở tầng `group`.

### Collap

#### K3 - System / Show

- `ad_sy_cl_sh_clDef`: tracking đầu vào của flow `show` tại tầng `system`.
- `ad_sy_cl_sh_clDef_n_iap`: drop do user đã mua Remove Ads, tương ứng `AdsLogic.IsRemovedAd = true`.
- `ad_sy_cl_sh_clDef_n_cfg`: drop do `configs == null` hoặc `configs.IsEnabled = false`.

#### K4 - AdCore / Show

- `ad_ac_cl_sh_clDef_n_group`: drop do không resolve được group để đi tiếp xuống tầng `group`.

#### K5 - Group / Show

- `ad_gr_sh_cl1234_clDef_n_nready`: drop do ad chưa ready tại tầng `group`.
- `ad_gr_sh_cl1234_clDef_n_showing`: drop do collap đang ở trạng thái showing.
- `ad_gr_sh_cl1234_clDef_api`: tracking khi flow `show` chạm API ở tầng `group`.

#### K8 - System / Request

- `ad_sy_cl_rq`: tracking đầu vào của flow `request` tại tầng `system`.
- `ad_sy_cl_rq_n_iap`: drop do user đã mua Remove Ads, tương ứng `AdsLogic.IsRemovedAd = true`.
- `ad_sy_cl_rq_n_cfg`: drop do `configs == null` hoặc `configs.IsEnabled = false`.

#### K9 - AdCore / Request

- `ad_ac_cl_rq_n_group`: drop do không resolve được group để đi tiếp xuống tầng `group`.

#### K10 - Group / Init

- `ad_gr_cl_rq_cl1234_ini_n_inited`: drop do group đã được initialize trước đó.
- `ad_gr_cl_rq_cl1234_ini_n_noid`: drop do thiếu ad unit id ở group.
- `ad_gr_cl_rq_cl1234_ini_n_aload`: drop do ad đã ở trạng thái loaded ngay trong bước init request.
- `ad_gr_cl_rq_cl1234_ini_api`: tracking khi flow `request` chạm API ở context `ini`.
- Với `Collap`, luồng init không đi qua `groupName`, nên request ở tầng `group` chỉ giữ `groupId = cl1234`.

#### K33 - AdCore / Hide

- `ad_sy_cl_hid_clDef`: system entry of hide flow.
- `ad_ac_cl_hid_clDef_n_group`: drop do không resolve được group để đi tiếp xuống tầng `group`.

#### K34 - Group / Hide

- `ad_gr_hid_cl1234_clDef_n_nshowing`: drop do collap hiện không ở trạng thái showing nên không có gì để hide.
- `ad_gr_hid_cl1234_clDef_api`: tracking khi flow `hide` chạm API ở tầng `group`.


