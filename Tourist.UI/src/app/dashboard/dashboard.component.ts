import { Component, ChangeDetectorRef } from '@angular/core';
import { Router } from '@angular/router';
import { CookieService } from 'ngx-cookie-service';
import { DashboardService } from './services/dashboard.service';
import { TouristPlace } from './models/tourist-place.model';

@Component({
  selector: 'app-dashboard',
  standalone: false,
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent {
  onSearchTermChange() {
    this.cdr.detectChanges();
  }
  // Helper to fetch places and run a callback after loading
  fetchPlacesWithCallback(cb: () => void) {
    this.loading = true;
    this.errorMsg = null;
    this.myPlaceIds.clear();
    this.dashboardService.getPlaces().subscribe({
      next: (data) => {
        const raw = (data || []) as any[];
        this.places = raw.map(p => ({
          ...p,
          id: p.id ?? p.touristPlaceId ?? p.placeId ?? p._id ?? p.Id ?? p.ID
        }));
        this.places = [...this.places];
  // ...existing code...
        for (const place of this.places) {
          const id = this.getPlaceId(place);
          const ownerEmail = this.extractCreatorEmail(place);
          const ownerId = this.extractCreatorId(place);
          const emailMatch = ownerEmail && this.normalizeEmail(ownerEmail) === this.normalizeEmail(this.userEmail || '');
          const idMatch = ownerId && this.currentUserId && String(ownerId) === String(this.currentUserId);
          if (id && (emailMatch || idMatch)) {
            this.myPlaceIds.add(id);
          }
        }
        // Force change detection after data update
        this.cdr.detectChanges();       
      },
      error: (err) => {
        this.errorMsg = err?.error?.message || 'Failed to load places';
      },
      complete: () => {
        this.loading = false;
        if (cb) cb();
      }
    });
  }
  userEmail: string | null = null;
  userRole: string | null = null;
  currentUserId: string | null = null;

  // Tourist Places logic
  places: TouristPlace[] = [];
  showPlaceForm = false;
  placePanelOpen = false;
  loading = false;
  errorMsg: string | null = null;
  successMsg: string | null = null;
  editingPlace: TouristPlace | null = null;
  placeForm: TouristPlace = {
    country: '',
    state: '',
    city: '',
    placeName: '',
    description: '',
    location: '',
    imageUrl: '',
    entryFee: 0
  };
  countries: any[] = [];
  states: any[] = [];
  districts: any[] = [];
  cities: any[] = [];
  selectedCountryCode: string = '';
  selectedStateCode: string = '';
  searchTerm: string = '';
  private myPlaceIds = new Set<string>();

  constructor(
    private router: Router,
    private cookieService: CookieService,
    private dashboardService: DashboardService,
    private cdr: ChangeDetectorRef,
  ) {}

  async ngOnInit() {
    this.userEmail = this.readUserEmail();
    this.userRole = this.readUserRole();
    this.currentUserId = this.readCurrentUserId();
    const { Country } = await import('country-state-city');
    this.countries = [Country.getCountryByCode('IN')].filter(Boolean);
    this.fetchPlaces();
  }

  private readUserEmail(): string | null {
    const keys = ['userEmail','email','Email','user_email','UserEmail'];
    for (const k of keys) {
      const v = localStorage.getItem(k);
      if (v) return v;
    }
    try {
      const cookieEmail = this.cookieService.get('userEmail') || this.cookieService.get('email');
      if (cookieEmail) return cookieEmail;
    } catch {}
    return null;
  }

  private readUserRole(): string | null {
    const keys = ['user-role','userRole','role','Role'];
    for (const k of keys) {
      const v = localStorage.getItem(k);
      if (v) return v;
    }
    try {
      const cookieRole = this.cookieService.get('user-role') || this.cookieService.get('role');
      if (cookieRole) return cookieRole;
    } catch {}
    return null;
  }

  private readCurrentUserId(): string | null {
    const keys = ['userId','user-id','userid','UserId','UserID','id','Id','ID'];
    for (const k of keys) {
      const v = localStorage.getItem(k);
      if (v) return v;
    }
    return null;
  }

  fetchPlaces() {
    this.loading = true;
    this.errorMsg = null;
    this.myPlaceIds.clear(); // Clear ownership set before rebuilding
    this.dashboardService.getPlaces().subscribe({
      next: (data) => {
        const raw = (data || []) as any[];
        // Normalize id field from various backend shapes
        // Always assign a new array reference to trigger Angular change detection
        this.places = raw.map(p => ({
          ...p,
          id: p.id ?? p.touristPlaceId ?? p.placeId ?? p._id ?? p.Id ?? p.ID
        }));
        this.places = [...this.places];
  // ...existing code...
        // Mark ownership for contributor visibility using creator email or id when available
        for (const place of this.places) {
          const id = this.getPlaceId(place);
          const ownerEmail = this.extractCreatorEmail(place);
          const ownerId = this.extractCreatorId(place);
          const emailMatch = ownerEmail && this.normalizeEmail(ownerEmail) === this.normalizeEmail(this.userEmail || '');
          const idMatch = ownerId && this.currentUserId && String(ownerId) === String(this.currentUserId);
          if (id && (emailMatch || idMatch)) {
            this.myPlaceIds.add(id);
          }
        }
        // Force change detection after data update
        this.cdr.detectChanges();    
        this.loading = false;
      },
      error: (err) => {
        this.errorMsg = err?.error?.message || 'Failed to load places';
        this.loading = false;
      }
    });
  }

  onCountryChange(event: Event) {
    const select = event.target as HTMLSelectElement;
    const code = (select.selectedOptions[0] as HTMLOptionElement).getAttribute('data-code') || '';
    this.selectedCountryCode = code;
    // placeForm.country already bound to name via ngModel
    import('country-state-city').then(({ State }) => {
      this.states = State.getStatesOfCountry(code);
      this.placeForm.state = '';
      this.selectedStateCode = '';
      this.cities = [];
      this.placeForm.city = '';
    });
  }

  onStateChange(event: Event) {
    const select = event.target as HTMLSelectElement;
    const code = (select.selectedOptions[0] as HTMLOptionElement).getAttribute('data-code') || '';
    this.selectedStateCode = code;
    // placeForm.state already bound to name via ngModel
    import('country-state-city').then(({ City }) => {
      this.cities = City.getCitiesOfState(this.selectedCountryCode, code);
      this.placeForm.city = '';
    });
  }

  isAdmin(): boolean { return (this.userRole || '').trim().toLowerCase() === 'admin'; }
  isContributor(): boolean { return (this.userRole || '').trim().toLowerCase() === 'contributor'; }
  isReader(): boolean { return (this.userRole || '').trim().toLowerCase() === 'reader'; }

  canAddOrUpdate(): boolean {
    return this.isAdmin() || this.isContributor();
  }

  canDelete(): boolean {
    return this.isAdmin();
  }

  canEditPlace(place: TouristPlace): boolean {
    if (this.isAdmin()) return true;
    if (!this.isContributor()) return false;
    const email = this.normalizeEmail(this.userEmail || '');
    const owner = this.normalizeEmail(this.extractCreatorEmail(place) || '');
    if (email && owner && email === owner) return true;
    const ownerId = this.extractCreatorId(place);
    if (ownerId && this.currentUserId && String(ownerId) === String(this.currentUserId)) return true;
    const id = this.getPlaceId(place);
    return !!(id && this.myPlaceIds.has(id));
  }

  private normalizeEmail(v: string): string {
    return (v || '').trim().toLowerCase();
  }

  private extractCreatorEmail(p: any): string | null {
    if (!p || typeof p !== 'object') return null;
    const keys = [
      'createdBy','CreatedBy','createdby','Createdby','created_by','creator','Creator',
      'createdByEmail','CreatedByEmail','createdbyEmail','CreatedbyEmail',
      'ownerEmail','OwnerEmail','email','Email','userEmail','UserEmail'
    ];
    for (const k of keys) {
      if (k in p && p[k]) return String(p[k]);
    }
    // Nested object support: createdBy: { email: '...' }
    if (p.createdBy && typeof p.createdBy === 'object') {
      const nested = p.createdBy;
      if (nested.email) return String(nested.email);
      if (nested.Email) return String(nested.Email);
    }
    // Nested object support: createdby: { email: '...' }
    if ((p as any).createdby && typeof (p as any).createdby === 'object') {
      const nested2 = (p as any).createdby;
      if (nested2.email) return String(nested2.email);
      if (nested2.Email) return String(nested2.Email);
      if (nested2.userEmail) return String(nested2.userEmail);
      if (nested2.UserEmail) return String(nested2.UserEmail);
    }
    return null;
  }

  private extractCreatorId(p: any): string | null {
    if (!p || typeof p !== 'object') return null;
    const keys = [
      'createdById','CreatedById','created_by_id','creatorId','CreatorId',
      'ownerId','OwnerId','userId','UserId','UserID'
    ];
    for (const k of keys) {
      if (k in p && p[k]) return String(p[k]);
    }
    // Nested object support: createdBy: { id: '...' }
    if (p.createdBy && typeof p.createdBy === 'object') {
      const nested = p.createdBy;
      if (nested.id) return String(nested.id);
      if (nested.Id) return String(nested.Id);
      if (nested.userId) return String(nested.userId);
      if (nested.UserId) return String(nested.UserId);
    }
    return null;
  }

  logout() {
    localStorage.removeItem('userEmail');
    localStorage.removeItem('user-role');
    this.cookieService.delete('Authorization', '/');
    this.router.navigate(['/login']);
  }

  openAddPlaceForm() {
    if (!this.canAddOrUpdate()) {
      this.errorMsg = 'You do not have permission to add tourist places.';
      return;
    }
    this.editingPlace = null;
    this.placeForm = {
      country: '', state: '', city: '', placeName: '', description: '', location: '', imageUrl: '', entryFee: 0
    };
    this.selectedCountryCode = '';
    this.selectedStateCode = '';
    this.states = [];
    this.cities = [];
    this.showPlaceForm = true;
  }

  async openEditPlaceForm(place: any) {
    if (!this.canEditPlace(place)) {
      this.errorMsg = 'You do not have permission to update tourist places.';
      return;
    }
    this.editingPlace = place;
    this.placeForm = { ...place };
    // Preload dropdowns based on stored names
    const { State, City, Country } = await import('country-state-city');
    if (!this.countries?.length) {
      this.countries = [Country.getCountryByCode('IN')].filter(Boolean);
    }
    const countryMatch = this.countries.find((c: any) => c.name === this.placeForm.country);
    this.selectedCountryCode = countryMatch?.isoCode || '';
    this.states = this.selectedCountryCode ? State.getStatesOfCountry(this.selectedCountryCode) : [];
    const stateMatch = this.states.find((s: any) => s.name === this.placeForm.state);
    this.selectedStateCode = stateMatch?.isoCode || '';
    this.cities = (this.selectedCountryCode && this.selectedStateCode) ? City.getCitiesOfState(this.selectedCountryCode, this.selectedStateCode) : [];
    this.showPlaceForm = true;
  }

  savePlace() {
    if (!this.canAddOrUpdate()) {
      this.errorMsg = 'You do not have permission to add or update tourist places.';
      return;
    }
    if (!this.placeForm.country || !this.placeForm.state || !this.placeForm.city || !this.placeForm.placeName || !this.placeForm.description || !this.placeForm.location || !this.placeForm.imageUrl) return;
    this.loading = true;
    this.errorMsg = null;
    if (this.editingPlace?.id) {
      if (!this.canEditPlace(this.editingPlace)) {
        this.errorMsg = 'You do not have permission to update this tourist place.';
        this.loading = false;
        return;
      }
      this.dashboardService.updatePlace(this.editingPlace.id, this.placeForm).subscribe({
        next: (resp) => {
          // Pure optimistic UI update using server response (if any)
          const updatedId = this.getPlaceId(this.editingPlace!);
          const merged = typeof resp === 'object' && resp !== null
            ? { ...this.editingPlace, ...this.placeForm, ...resp }
            : { ...this.editingPlace, ...this.placeForm };
          // Ensure id stays normalized
          merged.id = (this.getPlaceId(merged) ?? updatedId ?? '').toString();
          this.places = this.places.map(p => (this.getPlaceId(p) === updatedId ? { ...merged } : p));
          this.editingPlace = null;
          this.showPlaceForm = false;
          this.loading = false;
          this.successMsg = 'Tourist Place details updated successfully';
          this.cdr.detectChanges();
          setTimeout(() => { this.successMsg = null; this.cdr.detectChanges(); }, 2000);
        },
        error: (err) => {
          this.errorMsg = err?.error?.message || 'Failed to update place';
          this.loading = false;
        }
      });

// Helper to fetch places and run a callback after loading
    } else {
      const payload: any = { ...this.placeForm, createdBy: this.userEmail ?? this.placeForm.createdBy } as TouristPlace;
      // Also include lowercase 'createdby' to match backend expectations
      if (payload.createdBy && !payload.createdby) payload.createdby = payload.createdBy;
      this.dashboardService.addPlace(payload).subscribe({
        next: (created) => {
          const added: any = created && typeof created === 'object' ? { ...payload, ...created } : { ...payload };
          // Normalize id on the added object so trackBy works
          added.id = added.id ?? added.touristPlaceId ?? added.placeId ?? added._id ?? added.Id ?? added.ID ?? `tmp-${Date.now()}`;
          const newId = this.getPlaceId(added);
          if (newId) this.myPlaceIds.add(newId);
          // Optimistic UI update: prepend new item to the list
          this.places = [added, ...this.places];
          // Clear any active search so the new record isn't hidden by filter
          this.searchTerm = '';
          // Reset form for next entry
          this.placeForm = { country: '', state: '', city: '', placeName: '', description: '', location: '', imageUrl: '', entryFee: 0 } as TouristPlace;
          this.showPlaceForm = false;
          this.loading = false;
          this.successMsg = 'Place added successfully';
          this.cdr.detectChanges();
          setTimeout(() => { this.successMsg = null; this.cdr.detectChanges(); }, 2000);
        },
        error: (err) => {
          this.errorMsg = err?.error?.message || 'Failed to add place';
          this.loading = false;
        }
      });
    }
  }



  filteredPlaces(): TouristPlace[] {
    const term = (this.searchTerm || '').toLowerCase().trim();
    if (!term) return this.places;
    return this.places.filter(p => [
      p.placeName, p.country, p.state, p.city, p.location, p.description
    ].some(v => (v || '').toLowerCase().includes(term)));
  }

  getPlaceId(place: any): string | null {
    const pid = place?.id ?? place?.touristPlaceId ?? place?.placeId ?? place?._id ?? place?.Id ?? place?.ID;
    return pid != null ? String(pid) : null;
  }

  trackByPlace = (_: number, place: any) => this.getPlaceId(place) ?? _;

  // ...existing code...

  deletePlace(place: TouristPlace) {
    if (!this.canDelete()) {
      this.errorMsg = 'You do not have permission to delete tourist places.';
      return;
    }
    const id = this.getPlaceId(place);
    if (!id) {
      this.errorMsg = 'Cannot delete: missing place id.';
      return;
    }
    if (!confirm('Are you sure you want to delete this tourist place?')) {
      return;
    }
    this.loading = true;
    this.errorMsg = null;
    this.dashboardService.deletePlace(id).subscribe({
      next: () => {
        // Optimistically update the UI without waiting for a refetch
        this.places = this.places.filter(p => this.getPlaceId(p) !== id);
        this.loading = false;
      },
      error: (err) => {
        this.errorMsg = err?.error?.message || 'Failed to delete place';
        this.loading = false;
      }
    });
  }

  cancelPlaceForm() {
    this.showPlaceForm = false;
  }
}
