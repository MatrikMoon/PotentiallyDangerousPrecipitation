export class Semaphore {
    private tasks: (() => void)[] = [];
    private counter: number;

    constructor(maxConcurrency: number) {
        this.counter = maxConcurrency;
    }

    async acquire(): Promise<void> {
        if (this.counter > 0) {
            console.log('No pending tasks, allowing requested one');
            this.counter--;
            return;
        }

        console.log('Pending tasks, waiting');

        return new Promise((resolve) => {
            this.tasks.push(resolve);
        });
    }

    release(): void {
        console.log('Task completed, releasing');
        this.counter++;

        if (this.tasks.length > 0) {
            const nextTask = this.tasks.shift();
            if (nextTask) {
                this.counter--; // Reserve the slot for the next task
                console.log('Actually releasing');
                nextTask();
            }
        }
    }

    async use<T>(fn: () => Promise<T>): Promise<T> {
        await this.acquire();
        try {
            console.log('Running a task');
            return await fn();
        } finally {
            this.release();
        }
    }
}
