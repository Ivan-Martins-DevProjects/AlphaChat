import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [
    CommonModule
  ],
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.css',
})
export class Sidebar {
  @Input()
  open = false;

  activeItem = 'Inbox';
  activeSubItem = '';

  BodyItems = [
    { id: 'Inbox', icon: 'fa fa-envelope', route: '/home' },
    {
      id: 'Contacts', icon: 'fa fa-address-book', children: [
        { id: 'Todos', icon: 'fa fa-list', route: '/list-all-contacts' },
        { id: 'Carteira', icon: 'fa fa-wallet', route: '/wallet-contacts' },
      ]
    },
    { id: 'Teams', icon: 'fas fa-users', route: '' },
    { id: 'Labels', icon: 'fa fa-tags', route: '' },
  ]

  FooterItems = [
    { id: 'Settings', icon: 'fas fa-gears', route: '' },
    { id: 'Logout', icon: 'fa fa-sign-out', route: '' },
  ]

  constructor(private router: Router) {}

  setActive(item: string) {
    if (this.activeItem === item) {
      this.activeItem = "none"
      this.activeSubItem = ""
      return
    }

    this.activeItem = item;

    const found = this.BodyItems.find(i => i.id === item);
    if (found && !found.children && found.route) {
      this.router.navigate([found.route]);
    }
  }

  setSubActive(subItem: { id: string; route: string }) {
    this.activeSubItem = subItem.id;
    this.router.navigate([subItem.route]);
  }
}
