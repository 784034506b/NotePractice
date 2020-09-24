import { Injectable, Injector, Inject } from '@angular/core';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { zip } from 'rxjs';
import { catchError } from 'rxjs/operators';
import {
  MenuService,
  SettingsService,
  TitleService,
  ALAIN_I18N_TOKEN,
} from '@delon/theme';
import { ACLService } from '@delon/acl';
import { TranslateService } from '@ngx-translate/core';
import { I18NService } from '../i18n/i18n.service';
import { ReuseTabMatchMode, ReuseTabService } from '@delon/abc';

/**
 * 用于应用启动时
 * 一般用来获取应用所需要的基础数据等
 */
@Injectable()
export class StartupService {
  constructor(
    private menuService: MenuService,
    private translate: TranslateService,
    @Inject(ALAIN_I18N_TOKEN) private i18n: I18NService,
    private settingService: SettingsService,
    private aclService: ACLService,
    private titleService: TitleService,
    private httpClient: HttpClient,
    private injector: Injector,
    private reuseTabService: ReuseTabService,
  ) {}

  load(): Promise<any> {
    // only works with promises
    // https://github.com/angular/angular/issues/15088
    return new Promise((resolve, reject) => {
      // 设置reuseTabService.mode为ReuseTabMatchMode.URL
      // reuse-tab  是否重用页面,把这个页面排除了,所以只要跳转到别的页面 原tab就会消失
      // https://github.com/cipchk/ng-alain/issues/101
      this.reuseTabService.mode = ReuseTabMatchMode.URL;
      const excludes = new Array<RegExp>();
      excludes.push(new RegExp('/can-deactivate'));
      this.reuseTabService.excludes = excludes;

      zip(
        this.httpClient.get(`assets/tmp/i18n/${this.i18n.defaultLang}.json`),
        this.httpClient.get('assets/tmp/app-data.json'),
      )
        .pipe(
          // 接收其他拦截器后产生的异常消息
          catchError(([langData, appData]) => {
            resolve(null);
            return [langData, appData];
          }),
        )
        .subscribe(
          ([langData, appData]) => {
            // setting language data
            this.translate.setTranslation(this.i18n.defaultLang, langData);
            this.translate.setDefaultLang(this.i18n.defaultLang);

            // application data
            const res: any = appData;
            // 应用信息：包括站点名、描述、年份
            this.settingService.setApp(res.app);
            // 用户信息：包括姓名、头像、邮箱地址
            this.settingService.setUser(res.user);
            // ACL：设置权限为全量
            this.aclService.setFull(true);
             // 初始化菜单
            // 在后端创建子菜单
            res.menu.push({
              text: "",
              i18n: "子菜单",
              acl: "",
            });
            let index=res.menu.length-1;
            res.menu[index].children=[];
            res.menu[index].children.push({
              text: "导航1",
              i18n: "menu1",
              acl: "",
              link: "/menu1",
            });
            res.menu[index].children.push({
              text: "导航2",
              i18n: "menu2",
              acl: "",
              link: "/menu2",
            });
            this.menuService.add(res.menu);
            // 设置页面标题的后缀
            this.titleService.suffix = res.app.name;
          },
          () => {},
          () => {
            resolve(null);
          },
        );
    });
  }
}
