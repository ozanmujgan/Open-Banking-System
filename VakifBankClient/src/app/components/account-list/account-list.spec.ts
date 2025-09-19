import { ComponentFixture, TestBed } from '@angular/core/testing';

// If your component file is named 'account-list.component.ts', update the import as follows:
import { AccountListComponent } from './account-list.component';

describe('AccountList', () => {
  let component: AccountListComponent;
  let fixture: ComponentFixture<AccountListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [AccountListComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AccountListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
