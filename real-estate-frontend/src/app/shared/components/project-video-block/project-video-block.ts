import { isPlatformBrowser } from '@angular/common';
import { ChangeDetectionStrategy, Component, ElementRef, OnDestroy, PLATFORM_ID, ViewChild, ViewEncapsulation, computed, inject, input, signal } from '@angular/core';

@Component({
  selector: 'app-project-video-block',
  standalone: true,
  templateUrl: './project-video-block.html',
  styleUrl: './project-video-block.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: {
    '(document:click)': 'onDocumentClick($event)',
    '(document:keydown.escape)': 'onEscapePress()',
  },
})
export class ProjectVideoBlock implements OnDestroy {
  private readonly platformId = inject(PLATFORM_ID);
  private readonly isBrowser = isPlatformBrowser(this.platformId);

  videoUrl = input.required<string>();

  started = signal(false);
  playing = signal(false);
  muted = signal(false);
  settingsOpen = signal(false);
  playbackRate = signal(1);
  volumeLevel = signal(100);
  currentSeconds = signal(0);
  totalSeconds = signal(0);
  moreControlsOpen = signal(false);
  volumeMenuOpen = signal(false);

  readonly speedOptions = [0.75, 1, 1.25, 1.5, 2];

  @ViewChild('videoElement') videoElement!: ElementRef<HTMLVideoElement>;
  @ViewChild('videoFrame') videoFrame!: ElementRef<HTMLDivElement>;

  readonly progress = computed(() => {
    if (!this.totalSeconds()) {
      return 0;
    }

    return Math.min(100, (this.currentSeconds() / this.totalSeconds()) * 100);
  });

  readonly currentTimeLabel = computed(() => this.formatTime(this.currentSeconds()));
  readonly durationLabel = computed(() => this.formatTime(this.totalSeconds()));

  async playFromPoster(): Promise<void> {
    if (!this.isBrowser) {
      return;
    }

    this.started.set(true);
    await this.startVideo();
  }

  async togglePlayPause(): Promise<void> {
    if (!this.isBrowser) {
      return;
    }

    const video = this.videoElement?.nativeElement;
    if (!video) {
      return;
    }

    if (video.paused || video.ended) {
      if (!this.started()) {
        this.started.set(true);
      }
      await this.startVideo();
      return;
    }

    video.pause();
    this.settingsOpen.set(false);
  }

  toggleMute(): void {
    const video = this.videoElement?.nativeElement;
    if (!video) {
      return;
    }

    video.muted = !video.muted;
    if (!video.muted && video.volume === 0) {
      video.volume = 0.5;
      this.volumeLevel.set(50);
    }

    this.muted.set(video.muted);
    // Removed settingsOpen reset from here to allow it to stay open if needed,
    // but the user wants volume to be like speed settings (click to open).
  }

  toggleVolumeMenu(event: Event): void {
    event.stopPropagation();
    this.volumeMenuOpen.update(v => !v);
    this.settingsOpen.set(false);
    this.moreControlsOpen.set(false);
  }

  onVolumeInput(event: Event): void {
    const video = this.videoElement?.nativeElement;
    if (!video) {
      return;
    }

    const target = event.target as HTMLInputElement;
    const volumePercent = Number(target.value);
    if (!Number.isFinite(volumePercent)) {
      return;
    }

    const nextVolume = Math.max(0, Math.min(100, volumePercent));
    this.volumeLevel.set(nextVolume);
    video.volume = nextVolume / 100;
    video.muted = nextVolume === 0;
    this.muted.set(video.muted);
  }

  toggleSettingsMenu(event: Event): void {
    event.stopPropagation();
    this.settingsOpen.update((value) => !value);
    this.moreControlsOpen.set(false);
    this.volumeMenuOpen.set(false);
  }

  toggleMoreControls(event: Event): void {
    event.stopPropagation();
    this.moreControlsOpen.update((value) => !value);
    this.settingsOpen.set(false);
    this.volumeMenuOpen.set(false);
  }

  setPlaybackRate(rate: number): void {
    const video = this.videoElement?.nativeElement;
    this.playbackRate.set(rate);

    if (video) {
      video.playbackRate = rate;
    }

    this.settingsOpen.set(false);
  }

  seek(event: Event): void {
    const video = this.videoElement?.nativeElement;
    if (!video) {
      return;
    }

    const target = event.target as HTMLInputElement;
    const nextProgress = Number(target.value);
    if (!Number.isFinite(nextProgress)) {
      return;
    }

    video.currentTime = (nextProgress / 100) * (video.duration || 0);
    this.currentSeconds.set(video.currentTime || 0);
  }

  async toggleFullscreen(): Promise<void> {
    if (!this.isBrowser) {
      return;
    }

    const frame = this.videoFrame?.nativeElement;
    if (!frame) {
      return;
    }

    if (document.fullscreenElement === frame) {
      await document.exitFullscreen();
      this.settingsOpen.set(false);
      return;
    }

    if (document.fullscreenElement) {
      await document.exitFullscreen();
    }

    await frame.requestFullscreen();
    this.settingsOpen.set(false);
  }

  onLoadedMetadata(): void {
    const video = this.videoElement?.nativeElement;
    if (!video) {
      return;
    }

    this.totalSeconds.set(video.duration || 0);
    video.playbackRate = this.playbackRate();
    this.volumeLevel.set(Math.round((video.volume || 0) * 100));
    this.muted.set(video.muted || video.volume === 0);
  }

  onTimeUpdate(): void {
    const video = this.videoElement?.nativeElement;
    if (!video) {
      return;
    }

    this.currentSeconds.set(video.currentTime || 0);

    if (video.duration && this.totalSeconds() !== video.duration) {
      this.totalSeconds.set(video.duration);
    }
  }

  onPlay(): void {
    this.playing.set(true);
  }

  onPause(): void {
    this.playing.set(false);
  }

  onEnded(): void {
    this.playing.set(false);
    this.currentSeconds.set(this.totalSeconds());
    this.settingsOpen.set(false);
  }

  onVolumeChange(): void {
    const video = this.videoElement?.nativeElement;
    if (!video) {
      return;
    }

    this.volumeLevel.set(Math.round((video.volume || 0) * 100));
    this.muted.set(video.muted || video.volume === 0);
  }

  ngOnDestroy(): void {
    const video = this.videoElement?.nativeElement;
    if (video) {
      video.pause();
    }
  }

  onDocumentClick(event: MouseEvent): void {
    if (!this.settingsOpen()) {
      return;
    }

    const target = event.target as HTMLElement | null;
    if (!target) {
      this.settingsOpen.set(false);
      return;
    }

    if (target.closest('.video-settings-menu') ||
        target.closest('.video-settings-toggle') ||
        target.closest('.video-controls-group') ||
        target.closest('.video-more-toggle') ||
        target.closest('.video-volume-menu') ||
        target.closest('.video-volume-toggle')) {
      return;
    }

    this.settingsOpen.set(false);
    this.moreControlsOpen.set(false);
    this.volumeMenuOpen.set(false);
  }

  onEscapePress(): void {
    this.settingsOpen.set(false);
    this.moreControlsOpen.set(false);
    this.volumeMenuOpen.set(false);
  }

  private async startVideo(): Promise<void> {
    const video = this.videoElement?.nativeElement;
    if (!video) {
      return;
    }

    video.playbackRate = this.playbackRate();

    const playResult = video.play();

    if (playResult instanceof Promise) {
      try {
        await playResult;
      } catch {
        // Browser autoplay policies can reject promises before user media permissions settle.
      }
    }
  }

  private formatTime(value: number): string {
    const safeValue = Number.isFinite(value) ? Math.max(0, Math.floor(value)) : 0;
    const hours = Math.floor(safeValue / 3600);
    const minutes = Math.floor((safeValue % 3600) / 60);
    const seconds = safeValue % 60;

    const minutesPart = hours > 0 ? String(minutes).padStart(2, '0') : String(minutes);
    const secondsPart = String(seconds).padStart(2, '0');

    return hours > 0 ? `${hours}:${minutesPart}:${secondsPart}` : `${minutesPart}:${secondsPart}`;
  }
}
