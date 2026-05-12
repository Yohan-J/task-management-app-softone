import { Directive, OnDestroy } from '@angular/core';
import { Subject } from 'rxjs';

@Directive()
/**
 * Base class to ensure all component subscriptions are cancelled on destroy.
 */
export abstract class BaseComponent implements OnDestroy {
  protected readonly destroyed$ = new Subject<void>();

  ngOnDestroy(): void {
    this.destroyed$.next();
    this.destroyed$.complete();
  }
}
